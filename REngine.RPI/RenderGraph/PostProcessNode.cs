using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Features;

namespace REngine.RPI.RenderGraph
{
	public abstract class PostProcessNode : GraphicsRenderFeatureNode
	{
		private static readonly ulong sInputSlotHash = Hash.Digest("input");

		private static readonly ulong[] sExpectedWriteResources = { BackBufferSlotHash };
		private static readonly ulong[] sExpectedReadResources = { sInputSlotHash };

		private IResource? pReadTexture;
		private PostProcessFeature? pFeature;

		protected PostProcessNode(string debugName) : base(debugName)
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pFeature ??= GetPostProcessFeature();
			if (pReadTexture?.Value != null)
			{
				var readTexture = pReadTexture.Value switch
				{
					ITexture value => value.GetDefaultView(TextureViewType.ShaderResource),
					ITextureView valueView => valueView,
					_ => null
				};

				pFeature.ReadTexture = readTexture;
			}

			if (BackBufferResource?.Value != null)
			{
				var writeRt = BackBufferResource.Value switch
				{
					ITexture value => value.GetDefaultView(TextureViewType.RenderTarget),
					ITextureView valueView => valueView,
					_ => null
				};

				// Same case of Read Texture
				// In this case, we must get RenderTarget view type instead
				if(writeRt?.ViewType != TextureViewType.RenderTarget)
					writeRt = writeRt?.Parent.GetDefaultView(TextureViewType.RenderTarget);

				pFeature.WriteRT = writeRt;
			}

			base.OnRun(provider);
		}

		protected override void OnAddReadResource(ulong resourceSlotId, IResource resource)
		{
			if (resourceSlotId != sInputSlotHash)
				return;

			pReadTexture = resource;
			if (resource.Value == null) 
				return;
			var value = resource.Value;
			switch (value)
			{
				case ITexture tex:
				{
					if((tex.Desc.BindFlags & BindFlags.ShaderResource) == 0)
						ThrowInvalidResource();
					break;
				}
				case ITextureView texView:
				{
					if(texView.Desc.ViewType != TextureViewType.ShaderResource)
						ThrowInvalidResource();
					break;
				}
			}

			return;

			static void ThrowInvalidResource()
			{
				throw new InvalidOperationException(
					$"Expected {nameof(ITexture)} or {nameof(ITextureView)} as resource");
			}
		}

		protected override IEnumerable<ulong> GetExpectedReadResourceSlots()
		{
			return sExpectedReadResources;
		}

		protected override IEnumerable<ulong> GetExpectedWriteResourceSlots()
		{
			return sExpectedWriteResources;
		}

		protected override IRenderFeature GetFeature()
		{
			return GetPostProcessFeature();
		}
		protected abstract PostProcessFeature GetPostProcessFeature();
	}
}
