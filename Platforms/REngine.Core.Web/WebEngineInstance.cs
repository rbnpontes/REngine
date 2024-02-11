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

public class WebEngineInstance(IEngineApplication app, IWebStorage storage) : EngineInstance(app)
{
    private readonly GraphicsSettings pGraphicsSettings = new();
    private readonly RenderSettings pRenderSettings = new();
    protected override ILoggerFactory OnGetLoggerFactory()
    {
        return new WebLoggerFactory();
    }

    protected override void OnWriteSettings()
    {
         base.OnWriteSettings();
         WriteSettings(EngineSettings.GraphicsSettingsPath, pGraphicsSettings);
         WriteSettings(EngineSettings.RenderSettingsPath, pRenderSettings);
    }

    protected override void WriteSettings<T>(string path, T data)
    {
        storage.SetItem($"@rengine/{path}", data.ToJson());
    }

    protected override T LoadSettings<T>(string path)
    {
        var value = storage.GetItem($"@rengine/{path}");
        if (string.IsNullOrEmpty(value))
            return ActivatorExtended.CreateInstance<T>([]);
        return value.FromJson<T>() ?? ActivatorExtended.CreateInstance<T>([]);
    }

    protected override void OnSetupModules(List<IModule> modules)
    {
        modules.AddRange([
            new AssetsModule(),
            new WebModule(),
            new RHIModule(),
            new RPIModule(),
            new GameModule()
        ]);
        base.OnSetupModules(modules);
    }

    protected virtual IWindow OnCreateWindow(WebWindowManager windowManager)
    {
        return windowManager.Create("#canvas");
    }

    protected override void OnSetup(IServiceRegistry registry)
    {
        registry
            .Add(
                deps => OnCreateWindow((WebWindowManager)deps[0]),
                new Type[] { typeof(IWindowManager) })
            .Add(provider =>
            {
                var window = provider.Get<IWindow>();
                window.GetNativeWindow(out var nativeWindow);
                window.OnResize += OnWindowResize;
            
                Logger.Info(window);

                var (driver, swapChain) = DriverFactory.Build(new DriverFactoryCreateInfo()
                {
                    Window = nativeWindow,
                    MessageEvent = OnDriverMessage,
                    SwapChainDesc =  new SwapChainDesc(pGraphicsSettings)
                    {
                        Size = new SwapChainSize(window.Size),
                        Usage = SwapChainUsage.RenderTarget
                    }
                });

                pGraphicsSettings.DefaultColorFormat = swapChain.Desc.Formats.Color;
                pGraphicsSettings.DefaultDepthFormat = swapChain.Desc.Formats.Depth;

                registry.Add((_) => swapChain);
                return driver;
            });
    }

    protected override void RunGameLoop(IEngine engine)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        WebMarshal.CollectJsMemory();
        
        WebLooper.Build(ExecuteFrame);
        void ExecuteFrame(WebLooper looper)
        {
            if (engine.IsStopped)
            {
                ExecuteStop(looper);
                return;
            }

            engine.ExecuteFrame();
        }
        void ExecuteStop(WebLooper looper)
        {
            Logger.Info("Exiting App");
            App.OnExit(Provider);
            OnStop();
        }
    }
    
    private void OnDriverMessage(MessageEventData e)
    {
        switch (e.Severity)
        {
            case DbgMsgSeverity.Error:
            case DbgMsgSeverity.FatalError:
            case DbgMsgSeverity.Warning:
                Logger.Critical($"Diligent Engine: {e.Severity} in {e.Function}() ({e.File}, {e.Line}): {e.Message}");
                break;
            case DbgMsgSeverity.Info:
                Logger.Debug($"Diligent Engine: {e.Severity} {e.Message}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual void OnWindowResize(object sender, WindowResizeEventArgs eventArgs)
    {
        if (sender is not IWindow window)
            return;
        var swapChain = Provider.GetOrDefault<ISwapChain>();
        swapChain?.Resize(window.Size);
    }
}