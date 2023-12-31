using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI;

namespace REngine.Core.Android;

public abstract class App : IEngineApplication
{
    private ILogger? pLogger;
    private IWindow? pWindow;

    public ILogger Logger => pLogger ?? throw new NullReferenceException("Logger is null");
    public IWindow MainWindow => pWindow ?? throw new NullReferenceException("MainWindow is null");

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
        renderer.AddFeature(imGuiSystem.Feature, 1000/*ImGui Feature must execute at last*/);
#endif
    }
    
    private void HandleImGui(object? sender, EventArgs e)
    {
        OnGui();
    }
    
    protected virtual void OnGui() {}
    
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
}