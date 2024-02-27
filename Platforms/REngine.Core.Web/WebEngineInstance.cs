using REngine.Assets;
using REngine.Core.DependencyInjection;
using REngine.Core.Desktop;
using REngine.Core.IO;
using REngine.Core.Reflection;
using REngine.Core.Serialization;
using REngine.Game;
using REngine.RHI;
using REngine.RHI.Web.Driver;
using REngine.RPI;

namespace REngine.Core.Web;

public sealed class WebEngineInstance(IEngineApplication app, IWebStorage storage) : AsyncEngineInstance(app)
{
    private readonly GraphicsSettings pGraphicsSettings = new();
    private readonly RenderSettings pRenderSettings = new();
    private readonly WebLoggerFactory pWebLoggerFactory = new();
    protected override async Task<ILoggerFactory> OnGetLoggerFactory()
    {
        await Task.Yield();
        return pWebLoggerFactory;
    }

    protected override async Task OnWriteSettings()
    {
         await base.OnWriteSettings();
         await WriteSettings(EngineSettings.GraphicsSettingsPath, pGraphicsSettings);
         await WriteSettings(EngineSettings.RenderSettingsPath, pRenderSettings);
    }

    protected override async Task WriteSettings<T>(string path, T data)
    {
        await Task.Yield();
        storage.SetItem($"@rengine/{path}", data.ToJson());
    }

    protected override async Task<T> LoadSettings<T>(string path)
    {
        await Task.Yield();
        var value = storage.GetItem($"@rengine/{path}");
        if (string.IsNullOrEmpty(value))
            return ActivatorExtended.CreateInstance<T>([]);
        return value.FromJson<T>() ?? ActivatorExtended.CreateInstance<T>([]);
    }

    protected override Task OnSetupModules(List<IModule> modules)
    {
        modules.AddRange([
            new AssetsModule(),
            new WebModule(),
            new RHIModule(),
            new RPIModule(),
            new GameModule()
        ]);
        return base.OnSetupModules(modules);
    }

    private IWindow OnCreateWindow(WebWindowManager windowManager)
    {
        return windowManager.Create("#canvas");
    }

    protected override async Task OnSetup(IServiceRegistry registry)
    {
        await Task.Yield();
        registry
            .Add<ILoggerFactory>(()=> pWebLoggerFactory)
            .Add(
                deps => OnCreateWindow((WebWindowManager)deps[0]),
                new Type[] { typeof(IWindowManager) })
            .Add(provider =>
            {
                var window = provider.Get<IWindow>();
                window.GetNativeWindow(out var nativeWindow);
            
                Logger.Info(window);
                var (driver, swapChain) = DriverFactory.Build(new DriverFactoryCreateInfo()
                {
                    Window = nativeWindow,
                    MessageEvent = OnDriverMessage,
                    LoggerFactory = provider.Get<ILoggerFactory>(),
                    SwapChainDesc =  new SwapChainDesc(pGraphicsSettings)
                    {
                        Size = new SwapChainSize(window.Size),
                        Usage = SwapChainUsage.RenderTarget
                    }
                });

                window.OnResize += OnWindowResize;
                
                pGraphicsSettings.DefaultColorFormat = swapChain.Desc.Formats.Color;
                pGraphicsSettings.DefaultDepthFormat = swapChain.Desc.Formats.Depth;

                registry.Add((_) => swapChain);
                return driver;
            });
    }
    
    protected override async Task RunGameLoop(IEngine engine)
    {
        var taskCompletionSrc = new TaskCompletionSource<bool>();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        WebMarshal.CollectJsMemory();
        
        WebLooper.Build(ExecuteFrame, pWebLoggerFactory);
        await taskCompletionSrc.Task;
        Logger.Debug("Exiting");
        App.OnExit(Provider);
        await OnStop();
        Logger.Info("Finished!!!");
        
        return;
        void ExecuteFrame(WebLooper looper)
        {
            if (engine.IsStopped)
            {
                // ExecuteStop(looper);
                taskCompletionSrc.SetResult(true);
                return;
            }
        
            engine.ExecuteFrame();
        }
        // void ExecuteStop(WebLooper looper)
        // {
        //     Logger.Info("Exiting App");
        //     looper.Dispose();
        //     AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        //     App.OnExit(Provider);
        //     OnStop();
        // }
    }
    
    private void OnDriverMessage(MessageEventData e)
    {
        switch (e.Severity)
        {
            case DbgMsgSeverity.Error:
            case DbgMsgSeverity.FatalError:
                Logger.Critical($"Diligent Engine: {e.Severity} in {e.Function}() ({e.File}, {e.Line}): {e.Message}");
                break;
            case DbgMsgSeverity.Warning:
                Logger.Warning($"Diligent Engine: {e.Severity} {e.Message}");
                break;
            case DbgMsgSeverity.Info:
                Logger.Debug($"Diligent Engine: {e.Severity} {e.Message}");
                break;
        }
    }

    private void OnWindowResize(object sender, WindowResizeEventArgs eventArgs)
    {
        if (sender is not IWindow window)
            return;
        try
        {
            var swapChain = Provider.GetOrDefault<ISwapChain>();
            swapChain?.Resize(window.Size);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex.StackTrace);
        }
    }

    public static WebEngineInstance CreateStartup<T>() where T : IEngineApplication
    {
        if (ActivatorExtended.CreateInstance<T>([]) is not IEngineApplication app)
            throw new NullReferenceException($"Could not possible to create '{nameof(T)}'");
        return new WebEngineInstance(app, WebStorage.GetLocalStorage());
    }
}