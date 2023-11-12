using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public static class RPIModule
	{
		public static void Setup(IServiceRegistry registry)
		{
#if RENGINE_RENDERGRAPH
			RenderGraphModule.Setup(registry);
			var renderGraphRegistry = RenderGraphModule.GetBaseRegistry();
			renderGraphRegistry
				.Register<SpritebatchNode>()
				.Register<ImGuiNode>()
				.Register<ReadNode>()
				.Register<WriteNode>();
#endif
			registry
#if RENGINE_RENDERGRAPH
				.Add(()=> renderGraphRegistry)
#endif
				.Add<RenderSettings>()
				.Add<RPIEvents>()
				.Add<RenderState>()
				.Add(
					(deps) => ((ILoggerFactory)deps[0]).Build<IRenderer>(),
					new Type[] { typeof(ILoggerFactory) }
				)
				.Add(
					(deps) => ((ILoggerFactory)deps[0]).Build<IBufferManager>(),
					new Type[] { typeof(ILoggerFactory) }
				)
				.Add<IShaderManager, ShaderManagerImpl>()
				.Add<IPipelineStateManager, PipelineStateManagerImpl>()
				.Add<IBufferManager, BufferManager>()
				.Add<IRenderer, RendererImpl>()
				.Add<ITextRenderer, TextRendererImpl>()
#if RENGINE_SPRITEBATCH
				.Add<SpriteBatcher>()
				.Add<SpriteTextureManager>()
				.Add<ISpriteBatch, SpriteBatchImpl>()
#endif
#if RENGINE_IMGUI
				.Add<IImGuiSystem, ImGuiSystem>()
#endif
				.Add<BasicFeaturesFactory>();
		}
	}
}
