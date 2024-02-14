using System.Diagnostics;
using REngine.Core.DependencyInjection;
using REngine.Core.Events;
using REngine.Core.Exceptions;
using REngine.Core.IO;
using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.Core.Runtimes;
using REngine.Core.Serialization;
using REngine.Core.Threading;

namespace REngine.Core;

public abstract class AsyncEngineInstance(IEngineApplication app) : IAsyncEngineStartup
{
    private readonly Stopwatch pEngineStartTime = new();
    private readonly Stopwatch pSetupTime = new();
    private readonly Stopwatch pStartTime = new();

#if RENGINE_VALIDATIONS
    private EngineInstanceStep pCurrentStep = EngineInstanceStep.Setup;
#endif
    
    private ILoggerFactory? pLoggerFactory;
    private ILogger? pLogger;
    private IServiceProvider? pServiceProvider;

    protected IEngineApplication App => app;
    // ReSharper disable once MemberCanBeProtected.Global
    public IServiceProvider Provider
    {
        get
        {
            if (pServiceProvider is null)
                throw new NullReferenceException("Service Provider is null");
            return pServiceProvider;
        }
    }
    // ReSharper disable once MemberCanBeProtected.Global
    public ILogger Logger
    {
        get
        {
            if (pLogger is null)
                throw new NullReferenceException("Logger is null");
            return pLogger;
        }
    }
    
    protected abstract Task<ILoggerFactory> OnGetLoggerFactory();
    protected virtual async Task OnSetupModules(List<IModule> modules)
    {
        await Task.Yield();
        modules.Add(new CoreModule());
        app.OnSetupModules(modules);
    }
    protected virtual async Task OnSetupSettings(IServiceRegistry registry)
    {
        var engineSettings = await LoadSettings<EngineSettings>(EngineSettings.EngineSettingsPath);
        var assetManagerSettings = await LoadSettings<AssetManagerSettings>(EngineSettings.AssetManagerSettingsPath);
        
        await OnSetupEngineSettings(engineSettings);
        await OnSetupAssetManagerSettings(assetManagerSettings);
        registry.Add(() => engineSettings);
        registry.Add(() => assetManagerSettings);
    }
    protected virtual async Task OnSetupEngineSettings(EngineSettings engineSettings)
    {
        await Task.Yield();
        var processorCount = Math.Min(Environment.ProcessorCount, EngineSettings.MaxAllowedJobs);
        if (engineSettings.JobsThreadCount == -1)
            engineSettings.JobsThreadCount = processorCount;
        engineSettings.JobsThreadCount = Math.Clamp(engineSettings.JobsThreadCount, 0, processorCount);
    }
    protected virtual async Task OnSetupAssetManagerSettings(AssetManagerSettings settings)
    {
        await Task.Yield();
    }
    
    protected virtual async Task WriteSettings<T>(string path, T data)
    {
        if(File.Exists(path))
            File.Delete(path);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(data.ToJson());
    }
    protected virtual async Task<T> LoadSettings<T>(string path)
    {
        await using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return json.FromJson<T>() ?? ActivatorExtended.CreateInstance<T>([]);
    }
    
    public async Task Setup()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Setup);
#endif
        pEngineStartTime.Start();
        pSetupTime.Start();

        pLoggerFactory = await OnGetLoggerFactory();
        pLogger = pLoggerFactory.Build(GetType());

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        pLogger.Debug("Begin Setup Engine");

#if !WEB
        NativeReferences.Logger = pLoggerFactory.Build(typeof(NativeReferences));
        NativeReferences.PreloadLibs();
#endif

        pLogger.Debug("Set App Logger");
        app.OnSetLogger(pLogger);

        var registry = ServiceRegistryFactory.Build();
        var modules = new List<IModule>();

        pLogger.Debug("Begin OnSetupModules");
        await OnSetupModules(modules);
        pLogger.Debug("End OnSetupModules");

        pLogger.Debug("Executing Setup of Modules");
        modules.ForEach(x => x.Setup(registry));

        pLogger.Debug("Begin OnSetupSettings");
        await OnSetupSettings(registry);
        pLogger.Debug("End OnSetupSettings");

        pLogger.Debug("Begin OnSetup");
        await OnSetup(registry);
        pLogger.Debug("End OnSetup");

        pLogger.Debug("Begin App OnSetup");
        app.OnSetup(registry);
        pLogger.Debug("End App OnSetup");
        
        pLogger.Debug($"Execute {nameof(ApplicationLifecyle)} Setup");
        ApplicationLifecyle.ExecuteSetup(registry);

        pLogger.Debug("Building Service Provider");
        pServiceProvider = registry.Build();

        pLogger.Debug("Loading Execution Pipeline");
        var assetManager = pServiceProvider.Get<IAssetManager>();
        pServiceProvider.Get<IExecutionPipeline>().Load(
            await assetManager.GetAsyncStream("default_execution_pipeline.xml")
        );
        
        pSetupTime.Stop();
        pLogger.Debug("End Setup Engine");

#if RENGINE_VALIDATIONS
        pCurrentStep = EngineInstanceStep.Start;
#endif
    }

    protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex)
        {
            Logger.Critical($"Unhandled Exception: " + e.ExceptionObject);
            return;
        }

        Logger.Critical(ex.GetFullString());
    }

    protected virtual async Task OnSetup(IServiceRegistry registry)
    {
        await Task.Yield();
    }

    public async Task Start()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Start);
#endif
        ApplicationLifecyle.ExecuteStart(pServiceProvider);
        await OnStart();
        app.OnStart(Provider);
        
#if RENGINE_VALIDATIONS
        pCurrentStep = EngineInstanceStep.Run;
#endif
    }

    protected virtual async Task OnStart()
    {
        await Task.Yield();
    }

    public async Task Run()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Run);
#endif
        var engine = Provider.Get<IEngine>();
        var events = Provider.Get<EngineEvents>();
        events.OnUpdate += HandleUpdate;

        Logger.Debug("Starting");
        Logger.Info("OS Version: " + Environment.OSVersion);
        Logger.Info("Machine: " + Environment.MachineName);
        Logger.Info("TickCount:" + Environment.TickCount);
        Logger.Info("ProcessorCount: " + Environment.ProcessorCount);
        Logger.Info("UserName: " + Environment.UserName);
        Logger.Info("UserDomainName: " + Environment.UserDomainName);
        Logger.Info("App Data Path: " + EngineSettings.AppDataPath);
        Logger.Info("Log Path: " + EngineSettings.LoggerPath);

        pStartTime.Start();
        engine.Start();
        pStartTime.Stop();

        Logger.Debug("Starting Game Loop");

        ApplicationLifecyle.ExecuteRun();

        pEngineStartTime.Stop();

        Logger
            .Info($"Engine is Ready.")
            .Info($"Setup Time: {pSetupTime.Elapsed}")
            .Info($"Start Time: {pStartTime.Elapsed}")
            .Info($"Total Time: {pEngineStartTime.Elapsed}");

#if RENGINE_VALIDATIONS
        pCurrentStep = EngineInstanceStep.Stop;
#endif
        await OnReady();
        await RunGameLoop(engine);
    }

    protected virtual async Task OnReady()
    {
        await Task.Yield();
    }

    protected virtual async Task RunGameLoop(IEngine engine)
    {
        while (!engine.IsStopped)
            engine.ExecuteFrame();

        Logger.Debug("Exiting");
        
        app.OnExit(Provider);
        await OnStop();

        Logger.Info("Finished!!!");
    }
    
    private void HandleUpdate(object? sender, UpdateEventArgs args)
    {
        OnUpdate();
        app.OnUpdate(Provider);
    }

    protected virtual void OnUpdate() {}
    
    private bool pHasCallStop;
    public virtual async Task Stop()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Stop);
#endif
        await Task.Yield();
        if (pHasCallStop)
        {
            Logger.Warning("Stop has been already called. Skipping!!!");
            return;
        }

        pHasCallStop = true;
        Provider
            .Get<IExecutionPipeline>()
            .Invoke(() =>
            {
                Provider.Get<IEngine>().Stop();
            });
    }

    protected virtual async Task OnStop()
    {
#if PROFILER
        Profiler.Instance.Dispose();
#endif
        if(!Platform.IsWeb())
            NativeReferences.UnloadLibs();

        Logger.Info("Writing Settings Before Exit");
        await OnWriteSettings();
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    protected virtual async Task OnWriteSettings()
    {
        var engineSettings = Provider.GetOrDefault<EngineSettings>();
        var assetSettings = Provider.GetOrDefault<AssetManagerSettings>();
        
        if(engineSettings != null)
            await WriteSettings(EngineSettings.EngineSettingsPath, engineSettings);
        if(assetSettings != null)
            await WriteSettings(EngineSettings.AssetManagerSettingsPath, assetSettings);
    }
}