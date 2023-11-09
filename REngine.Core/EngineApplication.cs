using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface IEngineApplication
	{
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

			IEngine engine = pServiceProvider.Get<IEngine>();
			EngineEvents events = pServiceProvider.Get<EngineEvents>();
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

			CoreModule.Setup(registry);
			pApp.OnSetup(registry);

			pServiceProvider = registry.Build();
			using (FileStream stream = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/ExecutionPipeline.xml"), FileMode.Open))
				pServiceProvider.Get<IExecutionPipeline>().Load(stream);

			return this;
		}

		public IEngineStartup Start()
		{
			if (pServiceProvider is null)
				throw new NullReferenceException("IServiceProvider is null");

			pServiceProvider.Get<EngineEvents>()
				.ExecuteBeforeStart()
				.ExecuteStart();
			pApp.OnStart(pServiceProvider);
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
