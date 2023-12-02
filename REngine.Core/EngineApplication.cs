using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Events;

namespace REngine.Core
{
	public interface IEngineApplication
	{
		public void OnSetupModules(List<IModule> modules);
		public void OnSetup(IServiceRegistry registry);
		public void OnStart(IServiceProvider provider);
		public void OnUpdate(IServiceProvider provider);
		public void OnExit(IServiceProvider provider);
	}

	public interface IEngineStartup
	{
		public IEngineStartup Setup();
		public IEngineStartup Start();
		public IEngineStartup Run();
	}

	internal class EngineStartupImpl(IEngineApplication app) : IEngineStartup
	{
		private readonly Stopwatch pEngineStartTime = new();
		private readonly Stopwatch pSetupTime = new();
		private readonly Stopwatch pStartTime = new();

		private IServiceProvider? pServiceProvider;

		public IEngineStartup Run()
		{
			var loggerFactory = pServiceProvider.Get<ILoggerFactory>();
			var logger = loggerFactory.Build<IEngineStartup>();

			var engine = pServiceProvider.Get<IEngine>();
			var events = pServiceProvider.Get<EngineEvents>();
			events.OnUpdate += HandleUpdate;

			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				if (e.ExceptionObject is Exception exception)
					logger.Error(exception.Message);
				logger.Error(e.ExceptionObject.ToString());
			};

			logger.Debug("Starting");

			pStartTime.Start();
			engine.Start();
			pStartTime.Stop();

			logger.Debug("Starting Game Loop");

			ApplicationLifecyle.ExecuteRun();

			pEngineStartTime.Stop();

			logger
				.Info($"Engine is Ready.")
				.Info($"Setup Time: {pSetupTime.Elapsed}")
				.Info($"Start Time: {pStartTime.Elapsed}")
				.Info($"Total Time: {pEngineStartTime.Elapsed}");

			while (!engine.IsStopped)
				engine.ExecuteFrame();
			logger.Debug("Exiting");
			app.OnExit(pServiceProvider);
			return this;
		}

		private void HandleUpdate(object? sender, UpdateEventArgs args)
		{
			if (pServiceProvider is null)
				throw new NullReferenceException("IServiceProvider is null");
			app.OnUpdate(pServiceProvider);
		}

		public IEngineStartup Setup()
		{
			pEngineStartTime.Start();
			pSetupTime.Start();

			IServiceRegistry registry = ServiceRegistryFactory.Build();
			List<IModule> modules = new List<IModule>();

			SetupModules(modules);

			modules.ForEach(x => x.Setup(registry));

			app.OnSetup(registry);
			ApplicationLifecyle.ExecuteSetup(registry);

			pServiceProvider = registry.Build();
			using (FileStream stream = new(Path.Join(EngineSettings.AssetsPath, "default_execution_pipeline.xml"), FileMode.Open))
				pServiceProvider.Get<IExecutionPipeline>().Load(stream);

			pSetupTime.Stop();
			return this;
		}

		private void SetupModules(List<IModule> modules)
		{
			modules.Add(new CoreModule());
			app.OnSetupModules(modules);
		}

		public IEngineStartup Start()
		{
			if (pServiceProvider is null)
				throw new NullReferenceException("IServiceProvider is null");

			app.OnStart(pServiceProvider);
			ApplicationLifecyle.ExecuteStart(pServiceProvider);
			return this;
		}
	}

	public static class EngineApplication
	{
		public static IEngineStartup CreateStartup(IEngineApplication app)
		{
			return new EngineStartupImpl(app);
		}
		public static IEngineStartup CreateStartup<T>() where T: IEngineApplication
		{
			return new EngineStartupImpl(Activator.CreateInstance<T>());
		}
	}
}
