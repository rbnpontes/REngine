using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

#if RENGINE_RENDERGRAPH && RENGINE_IMGUI
namespace REngine.RPI.RenderGraph
{
	[NodeTag("imgui-pass")]
	public class ImGuiNode : GraphicsRenderFeatureNode
	{
		private static readonly ulong[] sExpectedWriteResources =
		{
			BackBufferSlotHash,
			DepthBufferSlotHash
		};

		private IGraphicsRenderFeature? pFeature;
		public ImGuiNode() : base(nameof(ImGuiNode)) { }

		protected override void OnRun(IServiceProvider provider)
		{
			pFeature ??= provider.Get<IImGuiSystem>().Feature;
			base.OnRun(provider);
		}

		protected override IRenderFeature GetFeature()
		{
			if (pFeature is null)
				throw new NullReferenceException("Render Feature is null, it seems Imgui Render Feature has not been loaded.");
			return pFeature;
		}

		protected override IEnumerable<ulong> GetExpectedWriteResourceSlots()
		{
			return sExpectedWriteResources;
		}
	}
}
#endif