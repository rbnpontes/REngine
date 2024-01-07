using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RPI.RenderGraph
{
	public abstract class GraphicsRenderFeatureNode : RenderFeatureNode
	{
		public const string BackBufferSlotName = "backbuffer";
		public const string DepthBufferSlotName = "depthbuffer";

		public static readonly ulong BackBufferSlotHash = Hash.Digest(BackBufferSlotName);
		public static readonly ulong DepthBufferSlotHash = Hash.Digest(DepthBufferSlotName);
		protected IResource? BackBufferResource { get; set; }
		protected IResource? DepthBufferResource { get; set; }

		private bool pDirtyBindings = false;

		protected GraphicsRenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			if(!pDirtyBindings)
			{
				base.OnExecute(command);
				return;
			}

			if(GetFeature() is IGraphicsRenderFeature feature)
			{
				TrySetBackBuffer(feature);
				TrySetDepthBuffer(feature);
				pDirtyBindings = false;
			}

			base.OnExecute(command);
		}

		private void TrySetBackBuffer(IGraphicsRenderFeature feature)
		{
			var value = BackBufferResource?.Value;
			feature.BackBuffer = value switch
			{
				ITexture texture => texture.GetDefaultView(TextureViewType.RenderTarget),
				ITextureView textureView => textureView,
				_ => feature.BackBuffer
			};
		}

		private void TrySetDepthBuffer(IGraphicsRenderFeature feature)
		{
			var value = DepthBufferResource?.Value;
			feature.DepthBuffer = value switch
			{
				ITexture texture => texture.GetDefaultView(TextureViewType.DepthStencil),
				ITextureView textureView => textureView,
				_ => feature.DepthBuffer
			};
		}

		protected override void OnAddWriteResource(ulong resourceSlotId, IResource resource)
		{
			if(resourceSlotId == BackBufferSlotHash && resource != BackBufferResource)
			{
				if (BackBufferResource != null)
					BackBufferResource.ValueChanged -= HandleResourceChanges;

				BackBufferResource = resource;
				pDirtyBindings = true;
			}

			if(resourceSlotId == DepthBufferSlotHash && resource != DepthBufferResource)
			{
				if(DepthBufferResource != null)
					DepthBufferResource.ValueChanged -= HandleResourceChanges;

				DepthBufferResource = resource;
				pDirtyBindings = true;
			}
			
			if(pDirtyBindings)
				resource.ValueChanged += HandleResourceChanges;
		}

		private void HandleResourceChanges(object? sender, ResourceChangeEventArgs e)
		{
			pDirtyBindings = true;
		}

		protected override void OnDispose()
		{
			if (BackBufferResource != null)
				BackBufferResource.ValueChanged -= HandleResourceChanges;
			if(DepthBufferResource != null)
				DepthBufferResource.ValueChanged -= HandleResourceChanges;
			base.OnDispose();
		}
	}
}
