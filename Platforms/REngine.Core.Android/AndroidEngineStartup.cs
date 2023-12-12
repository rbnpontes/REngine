using System.Diagnostics;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.Android.Windows;
using REngine.Assets;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RHI.NativeDriver;
using REngine.RPI;
using Activity = Android.App.Activity;
using WindowManager = REngine.Android.Windows.WindowManager;

namespace REngine.Core.Android;

internal class AndroidEngineInstance : EngineInstance
{
    private readonly ILoggerFactory pLoggerFactory;
    private readonly BaseGameActivity pActivity;
    private readonly REngine.Android.Windows.GameView pGameView;
    private readonly AssetManager pAssetManager;

    private readonly GraphicsSettings pGraphicsSettings = new();
    private readonly RenderSettings pRenderSettings = new();
    private readonly DriverSettings pDriverSettings = new();
    
    public AndroidEngineInstance(
        BaseGameActivity activity,
        REngine.Android.Windows.GameView gameView,
        AssetManager assetManager,
        IEngineApplication engineApplication) : base(engineApplication)
    {
#if DEBUG
        pLoggerFactory = new ComposedLoggerFactory([
            new AndroidLoggerFactory(),
            new FileLoggerFactory(EngineSettings.LoggerPath)
        ]);
#else
        pLoggerFactory = new AndroidLoggerFactory();
#endif
        pActivity = activity;
        pGameView = gameView;
        pAssetManager = assetManager;

        if(pActivity.CacheDir?.AbsolutePath is not null)
            EngineSettings.AppDataPath = pActivity.CacheDir.AbsolutePath;
    }
    
    protected override ILoggerFactory OnGetLoggerFactory()
    {
        return pLoggerFactory;
    }

    protected override void OnSetupModules(List<IModule> modules)
    {
        modules.AddRange([
            new WindowsModule(),
            new AssetsModule(),
            new RHIModule(),
            new RPIModule()
        ]);
        base.OnSetupModules(modules);
    }
    protected override void OnSetup(IServiceRegistry registry)
    {
        if (pActivity.ApplicationContext != null)
            registry.Add(() => pActivity.ApplicationContext);

        registry
            .Add(() => pLoggerFactory)
            .Add(() => pActivity)
            .Add(() => pGameView)
            .Add(() => pAssetManager)
            .Add<IAssetManager, AndroidAssetManager>()
            .Add<IEngine, AndroidEngine>()
            .Add(
                (deps) => OnCreateWindow((IWindowManager)deps[0]),
                [typeof(IWindowManager)]
            )
            .Add(provider =>
            {
                var window = provider.Get<IWindow>();
                window.GetNativeWindow(out var nativeWindow);
                window.OnResize +=HandleWindowResize;

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
    
    protected override void OnStart()
    {
        base.OnStart();

#if RENGINE_IMGUI
        var imGuiSystem = Provider.Get<IImGuiSystem>();
        var dpi = pActivity.GetDpi();
        // Scale UI to correctly DPI
        imGuiSystem.ScaleUi(dpi);
#endif
    }

    protected override void OnWriteSettings()
    {
        base.OnWriteSettings();
        
        WriteSettings(EngineSettings.GraphicsSettingsPath, pGraphicsSettings);
        WriteSettings(EngineSettings.RenderSettingsPath, pRenderSettings);
        WriteSettings(EngineSettings.DriverSettingsPath, pDriverSettings);
    }
    
    protected override void RunGameLoop(IEngine engine)
    {
        var gameHandler = new Handler(Looper.MainLooper);
        var runFrameAction = RunFrame;
        gameHandler.Post(runFrameAction);
        return;

        void RunFrame()
        {
            if (engine.IsStopped)
            {
                RunStop();
                return;
            }
            
            engine.ExecuteFrame();
            gameHandler.Post(RunFrame);
        }

        void RunStop()
        {
            Logger.Info("Exiting App");
            App.OnExit(Provider);
            OnStop();

            Logger.Info("Finished!!!");
        }
    }
    protected override void OnStop()
    {
        DriverFactory.OnDriverMessage -= OnDriverMessage;
        var window = Provider.GetOrDefault<IWindow>();
        if (window != null)
            window.OnResize -= HandleWindowResize;
        
        base.OnStop();
    }

    protected override void OnSetupEngineSettings(EngineSettings engineSettings)
    {
        base.OnSetupEngineSettings(engineSettings);
        engineSettings.JobsThreadCount = 0;
    }
    
    private void OnDriverMessage(object? sender, MessageEventArgs e)
    {
        switch (e.Severity)
        {
            case DbgMsgSeverity.Warning:
            case DbgMsgSeverity.Error:
            case DbgMsgSeverity.FatalError:
                Logger.Critical($"Diligent Engine: {e.Severity} in {e.Function}() ({e.File}, {e.Line}): {e.Message}");
                break;
            case DbgMsgSeverity.Info:
                Logger.Info($"Diligent Engine: {e.Severity} {e.Message}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleWindowResize(object sender, WindowResizeEventArgs e)
    {
        if (sender is not IWindow window)
            return;

        var swapChain = Provider.GetOrDefault<ISwapChain>();
        swapChain?.Resize(window.Size);
    }

    private IWindow OnCreateWindow(IWindowManager windowManager)
    {
        if (windowManager is not WindowManager wndMgr)
            throw new NullReferenceException(
                $"Can´t create Window Manager. {nameof(IWindowManager)} must be of {typeof(WindowManager).FullName}");
        return wndMgr.Create(pGameView);
    }
}