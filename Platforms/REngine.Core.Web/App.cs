using REngine.Core.DependencyInjection;
using REngine.Core.IO;

namespace REngine.Core.Web;

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
    }

    public virtual void OnUpdate(IServiceProvider provider)
    {
    }

    public virtual void OnExit(IServiceProvider provider)
    {
    }
    
    protected virtual void OnGui() {}
}