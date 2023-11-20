using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
#if RENGINE_RENDERGRAPH
	public static class NodeGraphsModule
	{
		public static void Setup(RenderGraphRegistry registry)
		{
			registry
				.Register<SpritebatchNode>()
				.Register<ImGuiNode>()
				.Register<ReadNode>()
				.Register<WriteNode>()
				.Register<RenderTargetNode>()
				.Register<ColorBufferNode>()
				.Register<DepthBufferNode>()
				.Register<ClearNode>()
				.Register<CopyRenderTargetNode>()
				.Register<GrayScalePostProcessNode>();
		}
	}
#endif
}
