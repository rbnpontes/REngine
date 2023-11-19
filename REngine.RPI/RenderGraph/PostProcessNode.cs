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
			pFeature.ReadTexture = (ITexture?)pReadTexture?.Value;
			pFeature.WriteRT = (ITextureView?)BackBufferResource?.Value;
			base.OnRun(provider);
		}

		protected override void OnAddReadResource(ulong resourceSlotId, IResource resource)
		{
			if (resourceSlotId != sInputSlotHash)
				return;

			pReadTexture = resource;
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
