using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;

namespace REngine.Core.Events
{
	public static class ApplicationLifecyle
	{
		public static readonly AsyncEventEmitter OnSetup = new();
		public static readonly AsyncEventEmitter OnStart = new();
		public static readonly EventEmitter OnRun = new();
		public static readonly AsyncEventEmitter OnExit = new();
		public static Task ExecuteSetup(IServiceRegistry registry) => OnSetup.Invoke(registry);

		public static Task ExecuteStart(IServiceProvider provider) => OnStart.Invoke(provider);

		public static void ExecuteRun(IEngineStartup engineStartup) => OnRun.Invoke(engineStartup);

		public static Task ExecuteExit(IEngine engine) => OnExit.Invoke(engine);
	}
}
