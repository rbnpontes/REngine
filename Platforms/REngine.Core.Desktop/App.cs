using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI;

namespace REngine.Core.Desktop;

public abstract class App : IEngineApplication
{
    private IWindow? pWindow;
    private ILogger? pLogger;
    
    public ILogger Logger => pLogger ?? throw new NullReferenceException();
    public IWindow MainWindow => pWindow ?? throw new NullReferenceException();
    public virtual void OnSetLogger(ILogger logger)
    {
        pLogger = logger;
    }

    public virtual void OnSetupModules(List<IModule> modules)
    {
    }

    public virtual void OnSetup(IServiceRegistry registry)
    {
    }

    public virtual void OnStart(IServiceProvider provider)
    {
        pWindow = provider.GetOrDefault<IWindow>();
#if RENGINE_IMGUI
        var renderer = provider.Get<IRenderer>();
        var imGuiSystem = provider.Get<IImGuiSystem>();
        
        imGuiSystem.OnGui += HandleImGui;
        renderer.AddFeature(imGuiSystem.Feature, 1000);
#endif
    }

    private void HandleImGui(object? sender, EventArgs e)
    {
        OnGui();
    }

    public virtual void OnUpdate(IServiceProvider provider)
    {
    }

    public virtual void OnExit(IServiceProvider provider)
    {
#if RENGINE_IMGUI
        var imGuiSystem = provider.Get<IImGuiSystem>();
        imGuiSystem.OnGui -= HandleImGui;
#endif
    }

    protected virtual void OnGui() {}
}