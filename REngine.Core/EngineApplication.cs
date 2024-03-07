using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Events;
using REngine.Core.Resources;
using REngine.Core.Runtimes;

namespace REngine.Core
{
	public interface IEngineApplication
	{
		public Task OnSetLogger(ILogger logger);
		public Task OnSetupModules(List<IModule> modules);
		public Task OnSetup(IServiceRegistry registry);
		public Task OnStart(IServiceProvider provider);
		public void OnUpdate(IServiceProvider provider);
		public Task OnExit(IServiceProvider provider);
	}

	public interface IEngineStartup
	{
		public Task Setup(IDispatcher dispatcher);
		public Task Start();
		public Task Run();
		public Task Stop();
	}

	/**
	 * A Helper Class used to Engine Startup
	 * in correctly order
	 * Its higher recommend to use this class
	 * due to Main Thread Calls
	 */
	public static class EngineApplication
	{
		public static void Run(IEngineStartup startup)
		{
			var dispatcher = DefaultDispatcher.Build(null);
			ExecuteStartup(startup, dispatcher);
			dispatcher.Run();
		}

		private static async void ExecuteStartup(IEngineStartup startup, IDispatcher dispatcher)
		{
			await startup.Setup(dispatcher);
			await startup.Start();
			await startup.Run();
		}
	}
}
