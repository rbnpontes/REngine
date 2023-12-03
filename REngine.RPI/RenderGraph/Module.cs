using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.RenderGraph.PostProcess;

namespace REngine.RPI.RenderGraph
{
#if RENGINE_RENDERGRAPH
    public static class NodeGraphsModule
	{
		public static void Setup(RenderGraphRegistry registry)
		{
			registry
				.Register<SpritebatchNode>()
#if RENGINE_IMGUI
				.Register<ImGuiNode>()
#endif
				.Register<ReadNode>()
				.Register<WriteNode>()
				.Register<RenderTargetNode>()
				.Register<ColorBufferNode>()
				.Register<DepthBufferNode>()
				.Register<ClearNode>()
				.Register<CopyRenderTargetNode>()
				.Register<GrayScalePostProcessNode>()
				.Register<InvertPostProcessNode>()
				.Register<SepiaPostProcessNode>()
				.Register<BlurNode>()
				.Register<CrtPostProcessNode>();
		}
	}
#endif
}
