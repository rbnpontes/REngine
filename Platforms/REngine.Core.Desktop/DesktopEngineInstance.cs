using System.Drawing;
using REngine.Assets;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Reflection;
using REngine.Game;
using REngine.RHI;
using REngine.RHI.NativeDriver;
using REngine.RPI;
using REngine.Windows;

namespace REngine.Core.Desktop;

public sealed class DesktopEngineInstance : EngineInstance
{
    private readonly GraphicsSettings pGraphicsSettings = new();
    private readonly RenderSettings pRenderSettings = new();
    private readonly DriverSettings pDriverSettings = new();

    private DesktopEngineInstance(IEngineApplication app) : base(app){}
    protected override ILoggerFactory OnGetLoggerFactory()
    {
#if DEBUG
        return new ComposedLoggerFactory([
            new DebugLoggerFactory(),
            new FileLoggerFactory(EngineSettings.LoggerPath)
        ]);
#else
        return new FileLoggerFactory(EngineSettings.LoggerPath);
#endif
    }
    protected override void OnWriteSettings()
    {
        base.OnWriteSettings();
        WriteSettings(EngineSettings.GraphicsSettingsPath, pGraphicsSettings);
        WriteSettings(EngineSettings.RenderSettingsPath, pRenderSettings);
        WriteSettings(EngineSettings.DriverSettingsPath, pDriverSettings);
    }
    protected override void OnSetupModules(List<IModule> modules)
    {
        modules.AddRange([
            new WindowsModule(),
            new AssetsModule(),
            new RHIModule(),
            new RPIModule(),
            new GameModule()
        ]);
        base.OnSetupModules(modules);
    }

    private IWindow OnCreateWindow(IWindowManager windowManager)
    {
        return windowManager.Create(new WindowCreationInfo()
        {
            Title = "REngine",
            Size = new Size(512, 512)
        });
    }
    protected override void OnSetup(IServiceRegistry registry)
    {
        registry
            .Add(
                deps => OnCreateWindow((IWindowManager)deps[0]),
                new Type[] { typeof(IWindowManager) }
            )
            .Add(provider =>
            {
                var window = provider.Get<IWindow>();
                window.GetNativeWindow(out var nativeWindow);
                // If main window goes to resize, we must update swapchain too
                // https://github.com/rbnpontes/REngine/issues/9
                window.OnResize += OnMainWindowResize;

                Logger.Info(window);

                DriverFactory.OnDriverMessage += OnDriverMessage;

                var driver = DriverFactory.Build(
                    pDriverSettings,
                    nativeWindow,
                    new SwapChainDesc(pGraphicsSettings)
                    {
                        Size = new SwapChainSize(window.Size),
                        Usage = SwapChainUsage.RenderTarget
                    }, out var swapChain);

                Logger.Info("GraphicsBackend: " + pDriverSettings.Backend);
                Logger.Info(driver.AdapterInfo);
                // When format is not supported by the driver
                // Driver will search for a compatible format
                // In this case we must update graphics settings

                if (swapChain == null) return driver;

                pGraphicsSettings.DefaultColorFormat = swapChain.Desc.Formats.Color;
                pGraphicsSettings.DefaultDepthFormat = swapChain.Desc.Formats.Depth;

                registry.Add((_) => swapChain);                
                return driver;
            });
    }
    private void OnMainWindowResize(object sender, WindowResizeEventArgs e)
    {
        if (sender is not IWindow window)
            return;
        
        var swapChain = Provider.GetOrDefault<ISwapChain>();
        swapChain?.Resize(window.Size);
    }
    private void OnDriverMessage(object? sender, MessageEventArgs e)
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
    protected override void OnStop()
    {
        DriverFactory.OnDriverMessage -= OnDriverMessage;
        var window = Provider.GetOrDefault<IWindow>();
        if(window is not null)
            window.OnResize -= OnMainWindowResize;
        base.OnStop();
    }
    protected override void OnStart()
    {
        var window = Provider.GetOrDefault<IWindow>();
        window?.Show();
    }
    protected override void OnSetupSettings(IServiceRegistry registry)
    {
        base.OnSetupSettings(registry);
        pGraphicsSettings.Merge(LoadSettings<GraphicsSettings>(EngineSettings.GraphicsSettingsPath));
        pRenderSettings.Merge(LoadSettings<RenderSettings>(EngineSettings.RenderSettingsPath));
        pDriverSettings.Merge(LoadSettings<DriverSettings>(EngineSettings.DriverSettingsPath));
        
        registry
            .Add(() => pGraphicsSettings)
            .Add(() => pRenderSettings)
            .Add(() => pDriverSettings);
    }

    public static DesktopEngineInstance CreateStartup(Type appType)
    {
        if (!appType.IsAssignableTo(typeof(IEngineApplication)))
            throw new ArgumentException($"appType argument must implement {nameof(IEngineApplication)}.");
        if (ActivatorExtended.CreateInstance(appType, []) is not IEngineApplication app)
            throw new NullReferenceException($"Could not possible to create '{appType.Name}'");
        return new DesktopEngineInstance(app);
    }
    public static DesktopEngineInstance CreateStartup(IEngineApplication app)
    {
        return new DesktopEngineInstance(app);
    }
    public static DesktopEngineInstance CreateStartup<T>() where T : IEngineApplication
    {
        if (ActivatorExtended.CreateInstance<T>([]) is not IEngineApplication app)
            throw new NullReferenceException($"Could not possible to create '{nameof(T)}'");
        return new DesktopEngineInstance(app);
    }
}