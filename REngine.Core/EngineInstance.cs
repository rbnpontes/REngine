using System.Diagnostics;
using System.Text;
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

public abstract class EngineInstance(IEngineApplication app) : IEngineStartup
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

    public IEngineApplication App => app;
    public IServiceProvider Provider
    {
        get
        {
            if (pServiceProvider is null)
                throw new NullReferenceException("Service Provider is null");
            return pServiceProvider;
        }
    }

    public ILogger Logger
    {
        get
        {
            if (pLogger is null)
                throw new NullReferenceException("Logger is null");
            return pLogger;
        }
    }

    protected abstract ILoggerFactory OnGetLoggerFactory();

    public virtual IEngineStartup Run()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Run);
#endif
        var engine = Provider.Get<IEngine>();
        var events = Provider.Get<EngineEvents>();
        events.OnUpdate += HandleUpdate;

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception exception)
                Logger.Error(exception.Message);
            Logger.Error(e.ExceptionObject.ToString());
        };

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
        OnReady();
        RunGameLoop(engine);
        return this;
    }

    protected virtual void RunGameLoop(IEngine engine)
    {
        while (!engine.IsStopped)
            engine.ExecuteFrame();
        
        Logger.Debug("Exiting");
        app.OnExit(Provider);
        OnStop();

        Logger.Info("Finished!!!");
    }
    
    private void HandleUpdate(object? sender, UpdateEventArgs args)
    {
        OnUpdate();
        app.OnUpdate(Provider);
    }
    protected virtual void OnUpdate()
    {
    }
    public virtual IEngineStartup Setup()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Setup);
#endif
        
        pEngineStartTime.Start();
        pSetupTime.Start();

        pLoggerFactory = OnGetLoggerFactory();
        pLogger = pLoggerFactory.Build(GetType());

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        pLogger.Debug("Begin Setup Engine");
#if WEB
        if (Platform.IsWeb())
            throw new PlatformNotSupportedException(
                $"{nameof(EngineInstance)} is not supported on Web. Please use {nameof(AsyncEngineInstance)} instead!");
#endif
        
        NativeReferences.Logger = pLoggerFactory.Build(typeof(NativeReferences));
        NativeReferences.PreloadLibs();

        pLogger.Debug("Set App Logger");
        app.OnSetLogger(pLogger);

        var registry = ServiceRegistryFactory.Build();
        var modules = new List<IModule>();
        
        pLogger.Debug("Begin OnSetupModules");
        OnSetupModules(modules);
        pLogger.Debug("End OnSetupModules");
        
        pLogger.Debug("Executing Setup of Modules");
        modules.ForEach(x => x.Setup(registry));

        pLogger.Debug($"Begin {nameof(OnSetupSettings)}");
        OnSetupSettings(registry);
        pLogger.Debug($"End {nameof(OnSetupSettings)}");
        
        pLogger.Debug($"Begin {nameof(OnSetup)}");
        OnSetup(registry);
        pLogger.Debug($"End {nameof(OnSetup)}");
        
        pLogger.Debug($"Begin {nameof(app.OnSetup)}");
        app.OnSetup(registry);
        pLogger.Debug($"End {nameof(app.OnSetup)}");
        
        pLogger.Debug($"Execute {nameof(ApplicationLifecyle)} Setup");
        ApplicationLifecyle.ExecuteSetup(registry);

        pLogger.Debug("Building Service Provider");
        pServiceProvider = registry.Build();

        pLogger.Debug("Loading Execution Pipeline");
        var assetManager = pServiceProvider.Get<IAssetManager>();
        pServiceProvider.Get<IExecutionPipeline>().Load(
            assetManager.GetStream("default_execution_pipeline.xml")
        );

        pSetupTime.Stop();
        pLogger.Debug("End Setup Engine");

#if RENGINE_VALIDATIONS
        pCurrentStep = EngineInstanceStep.Start;
#endif
        return this;
    }

    protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex)
        {
            Logger.Critical($"Unhandled Exception: " + e.ExceptionObject.ToString());
            return;
        }

        Logger.Critical(ex.GetFullString());
    }

    protected virtual void OnSetup(IServiceRegistry registry)
    {
    }
    protected virtual void OnSetupModules(List<IModule> modules)
    {
        modules.Add(new CoreModule());
        app.OnSetupModules(modules);
    }
    protected virtual void OnSetupSettings(IServiceRegistry registry)
    {
        var engineSettings = LoadSettings<EngineSettings>(EngineSettings.EngineSettingsPath);
        var assetManagerSettings = LoadSettings<AssetManagerSettings>(EngineSettings.AssetManagerSettingsPath);
        
        OnSetupEngineSettings(engineSettings);
        OnSetupAssetManagerSettings(assetManagerSettings);
        registry.Add(() => engineSettings);
        registry.Add(() => assetManagerSettings);
    }

    protected virtual void OnSetupEngineSettings(EngineSettings engineSettings)
    {
        var processorCount = Math.Min(Environment.ProcessorCount, EngineSettings.MaxAllowedJobs);
        if (engineSettings.JobsThreadCount == -1)
            engineSettings.JobsThreadCount = processorCount;
        engineSettings.JobsThreadCount = Math.Clamp(engineSettings.JobsThreadCount, 0, processorCount);
    }

    protected virtual void OnSetupAssetManagerSettings(AssetManagerSettings settings)
    {
    }

    public IEngineStartup Start()
    {
#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Start);
#endif
        ApplicationLifecyle.ExecuteStart(Provider);
        OnStart();
        app.OnStart(Provider);

#if RENGINE_VALIDATIONS
        pCurrentStep = EngineInstanceStep.Stop;
#endif
        return this;
    }

    private bool pHasCallStop;
    public IEngineStartup Stop()
    {
        if (pHasCallStop)
        {
            Logger.Warning("Stop has been already called. Skipping!!!");
            return this;
        }

#if RENGINE_VALIDATIONS
        InvalidEngineInstanceCallException.Validate(pCurrentStep, EngineInstanceStep.Stop);
#endif
        
        pHasCallStop = true;
        
        Provider
            .Get<IExecutionPipeline>()
            .Invoke(() =>
            {
                Provider.Get<IEngine>().Stop();
            });
        return this;
    }
    
    protected virtual void OnStart(){}

    protected virtual void OnStop()
    {
#if PROFILER
        Profiler.Instance.Dispose();
#endif
        if(!Platform.IsWeb())
            NativeReferences.UnloadLibs();

        Logger.Info("Writing Settings Before Exit");
        OnWriteSettings();
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    protected virtual void OnWriteSettings()
    {
        var engineSettings = Provider.GetOrDefault<EngineSettings>();
        var assetSettings = Provider.GetOrDefault<AssetManagerSettings>();
        
        if(engineSettings != null)
            WriteSettings(EngineSettings.EngineSettingsPath, engineSettings);
        if(assetSettings != null)
            WriteSettings(EngineSettings.AssetManagerSettingsPath, assetSettings);
    }

    protected virtual void WriteSettings<T>(string path, T data)
    {
        if(File.Exists(path))
            File.Delete(path);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(data.ToJson());
    }

    protected virtual T LoadSettings<T>(string path)
    {
        using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return json.FromJson<T>() ?? ActivatorExtended.CreateInstance<T>([]);
    }
    
    protected virtual void OnReady()
    {
    }
}