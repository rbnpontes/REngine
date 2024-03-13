using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using REngine.RPI;

namespace REngine.Core.Desktop;

public abstract class App : IEngineApplication
{
    private IWindow? pWindow;
    private ILogger? pLogger;
    
    public ILogger Logger => pLogger ?? throw new NullReferenceException();
    public IWindow MainWindow => pWindow ?? throw new NullReferenceException();
    public virtual async Task OnSetLogger(ILogger logger)
    {
        await Task.Yield();
        pLogger = logger;
    }

    public virtual async Task OnSetupModules(List<IModule> modules) => await Task.Yield();

    public virtual async Task OnSetup(IServiceRegistry registry) => await Task.Yield();

    public virtual async Task OnStart(IServiceProvider provider)
    {
        await Task.Yield();
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

    public virtual async Task OnExit(IServiceProvider provider)
    {
        await Task.Yield();
#if RENGINE_IMGUI
        var imGuiSystem = provider.Get<IImGuiSystem>();
        imGuiSystem.OnGui -= HandleImGui;
#endif
    }

    protected virtual void OnGui() {}
}