using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Core;
using REngine.Core.Runtimes;
using REngine.RPI.Events;

namespace REngine.RPI
{
    // ReSharper disable once InconsistentNaming
    public sealed class RPIModule : IModule
    {
        public void Setup(IServiceRegistry registry)
        {
            try
            {
                // If Import has been added, we must skip
                NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, NativeReferences.DefaultDllImportResolver);
            }
            catch
            {
                // ignored
            }
#if RENGINE_RENDERGRAPH
            RenderGraphModule.Setup(registry);
            var renderGraphRegistry = RenderGraphModule.GetBaseRegistry();
            NodeGraphsModule.Setup(renderGraphRegistry);
#endif

            EventsModule.Setup(registry);

            registry
#if RENGINE_RENDERGRAPH
                .Add(() => renderGraphRegistry)
#endif
                .Add<RenderSettings>()
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
                .Add<IBufferManager, BufferManagerImpl>()
                .Add<IRenderTargetManager, RenderTargetManagerImpl>()
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