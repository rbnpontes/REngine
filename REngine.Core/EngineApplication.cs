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
		public Task Setup();
		public Task Start();
		public Task Run();
		public Task Stop();
	}

	/**
	 * A Helper Class used to Engine Startup
	 * in correctly order
	 */
	public static class EngineApplication
	{
		public static async Task Run(IEngineStartup startup)
		{
			await startup.Setup();
			await startup.Start();
			await startup.Run();
		}
	}
}
