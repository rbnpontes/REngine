using System.Diagnostics;
using REngine.Core.DependencyInjection;
using REngine.Core.Events;
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

    private ILoggerFactory? pLoggerFactory;
    private ILogger? pLogger;
    private IServiceProvider? pServiceProvider;

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

        OnReady();

        RunGameLoop(engine);
        
        Logger.Debug("Exiting");
        app.OnExit(Provider);
        OnStop();
        return this;
    }

    protected virtual void RunGameLoop(IEngine engine)
    {
        while (!engine.IsStopped)
            engine.ExecuteFrame();
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
        pEngineStartTime.Start();
        pSetupTime.Start();

        pLoggerFactory = OnGetLoggerFactory();
        pLogger = pLoggerFactory.Build(GetType());

        NativeReferences.Logger = pLoggerFactory.Build(typeof(NativeReferences));
        NativeReferences.PreloadLibs();

        app.OnSetLogger(pLoggerFactory.Build(app.GetType()));

        var registry = ServiceRegistryFactory.Build();
        var modules = new List<IModule>();

        OnSetupModules(modules);

        modules.ForEach(x => x.Setup(registry));

        OnSetupSettings(registry);
        OnSetup(registry);
        app.OnSetup(registry);
        ApplicationLifecyle.ExecuteSetup(registry);

        pServiceProvider = registry.Build();

        var assetManager = pServiceProvider.Get<IAssetManager>();
        pServiceProvider.Get<IExecutionPipeline>().Load(
            assetManager.GetStream("default_execution_pipeline.xml")
        );

        pSetupTime.Stop();
        return this;
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
        ApplicationLifecyle.ExecuteStart(Provider);
        OnStart();
        app.OnStart(Provider);
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
        NativeReferences.UnloadLibs();

        Logger.Info("Writing Settings Before Exit");
        OnWriteSettings();
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

    protected static void WriteSettings<T>(string path, T data)
    {
        if(File.Exists(path))
            File.Delete(path);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream);
        writer.Write(data.ToJson());
    }

    protected static T LoadSettings<T>(string path)
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