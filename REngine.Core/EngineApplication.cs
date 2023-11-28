using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
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

	internal class EngineStartupImpl : IEngineStartup
	{
		private readonly IEngineApplication pApp;
		private IServiceProvider? pServiceProvider;

		public EngineStartupImpl(IEngineApplication app)
		{
			pApp = app;
		}

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
			engine.Start();
			logger.Debug("Starting Game Loop");

			ApplicationLifecyle.ExecuteRun();
			while (!engine.IsStopped)
				engine.ExecuteFrame();
			logger.Debug("Exiting");
			pApp.OnExit(pServiceProvider);
			return this;
		}

		private void HandleUpdate(object? sender, UpdateEventArgs args)
		{
			if (pServiceProvider is null)
				throw new NullReferenceException("IServiceProvider is null");
			pApp.OnUpdate(pServiceProvider);
		}

		public IEngineStartup Setup()
		{
			IServiceRegistry registry = ServiceRegistryFactory.Build();
			List<IModule> modules = new List<IModule>();
			SetupModules(modules);

			modules.ForEach(x => x.Setup(registry));

			pApp.OnSetup(registry);
			ApplicationLifecyle.ExecuteSetup(registry);

			pServiceProvider = registry.Build();
			using (FileStream stream = new(Path.Join(EngineSettings.AssetsPath, "default_execution_pipeline.xml"), FileMode.Open))
				pServiceProvider.Get<IExecutionPipeline>().Load(stream);

			return this;
		}

		private void SetupModules(List<IModule> modules)
		{
			modules.Add(new CoreModule());
			pApp.OnSetupModules(modules);
		}

		public IEngineStartup Start()
		{
			if (pServiceProvider is null)
				throw new NullReferenceException("IServiceProvider is null");

			pApp.OnStart(pServiceProvider);
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
