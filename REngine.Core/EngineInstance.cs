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

    protected abstract Task<ILoggerFactory> OnGetLoggerFactory();

    public virtual async Task Run()
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
        await engine.Start();
        pStartTime.Stop();

        Logger.Info("Setup Final Touches");

        ApplicationLifecyle.ExecuteRun();

        pEngineStartTime.Stop();

        Logger
            .Info($"Engine is Ready.")
            .Info($"Setup Time: {pSetupTime.Elapsed}")
            .Info($"Start Time: {pStartTime.Elapsed}")
            .Info($"Total Time: {pEngineStartTime.Elapsed}");
        
        await OnReady();

        Logger.Info("Starting Game Loop");
        RunGameLoop(engine);
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
    public virtual async Task Setup()
    {
        pEngineStartTime.Start();
        pSetupTime.Start();

        pLoggerFactory = await OnGetLoggerFactory();
        pLogger = pLoggerFactory.Build(GetType());

        NativeReferences.Logger = pLoggerFactory.Build(typeof(NativeReferences));
        NativeReferences.PreloadLibs();

        await app.OnSetLogger(pLoggerFactory.Build(app.GetType()));

        var registry = ServiceRegistryFactory.Build();
        var modules = new List<IModule>();

        pLogger.Info("Setup Modules");
        await OnSetupModules(modules);

        modules.ForEach(x => x.Setup(registry));

        pLogger.Info("Setup Settings");
        await OnSetupSettings(registry);

        pLogger.Info("Setup System");
        await OnSetup(registry);
        pLogger.Info($"Setup Application '{(app.GetType().FullName ?? app.GetType().Name)}'");
        await app.OnSetup(registry);

        pLogger.Info("Emitting Setup Event");
        ApplicationLifecyle.ExecuteSetup(registry);

        pLogger.Info("Building Service Provider");
        pServiceProvider = registry.Build();
        pLogger.Success("Service Provider is Ready!");

        var assetManager = pServiceProvider.Get<IAssetManager>();

        pLogger.Info("Loading Default Execution Pipeline Layout");
        pServiceProvider.Get<IExecutionPipeline>().Load(
            assetManager.GetStream("default_execution_pipeline.xml")
        );

        pSetupTime.Stop();
        pLogger.Info("Setup is Finished!");
    }

    protected virtual async Task OnSetup(IServiceRegistry registry) => await Task.Yield();
    protected virtual Task OnSetupModules(List<IModule> modules)
    {
        modules.Add(new CoreModule());
        return app.OnSetupModules(modules);
    }
    protected virtual async Task OnSetupSettings(IServiceRegistry registry)
    {
        var engineSettings = LoadSettings<EngineSettings>(EngineSettings.EngineSettingsPath);
        var assetManagerSettings = LoadSettings<AssetManagerSettings>(EngineSettings.AssetManagerSettingsPath);

        await Task.WhenAll(engineSettings, assetManagerSettings);
        await Task.WhenAll(
            OnSetupEngineSettings(engineSettings.Result),
            OnSetupAssetManagerSettings(assetManagerSettings.Result)
        );
        
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

    protected virtual async Task OnSetupAssetManagerSettings(AssetManagerSettings settings) => await Task.Yield();

    public async Task Start()
    {
        ApplicationLifecyle.ExecuteStart(Provider);
        await OnStart();
        await app.OnStart(Provider);
    }

    private bool pHasCallStop;
    public virtual async Task Stop()
    {
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

    protected virtual async Task OnStart() => await Task.Yield();

    protected virtual Task OnStop()
    {
#if PROFILER
        Profiler.Instance.Dispose();
#endif
        NativeReferences.UnloadLibs();

        Logger.Info("Writing Settings Before Exit");
        return OnWriteSettings();
    }

    protected virtual async Task OnWriteSettings()
    {
        var engineSettings = Provider.GetOrDefault<EngineSettings>();
        var assetSettings = Provider.GetOrDefault<AssetManagerSettings>();

        var tasks = new List<Task>();
        if(engineSettings != null)
            tasks.Add(WriteSettings(EngineSettings.EngineSettingsPath, engineSettings));
        if(assetSettings != null)
            tasks.Add(WriteSettings(EngineSettings.AssetManagerSettingsPath, assetSettings));
        
        await Task.WhenAll(tasks);
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

    protected virtual async Task OnReady() => await Task.Yield();
}