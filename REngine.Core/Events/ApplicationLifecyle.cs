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
		public static event EventHandler<IServiceRegistry>? OnSetup;
		public static event EventHandler<IServiceProvider>? OnStart;
		public static event EventHandler? OnRun;
		public static event EventHandler? OnExit;
		public static void ExecuteSetup(IServiceRegistry registry)
		{
			OnSetup?.Invoke(null, registry);
		}

		public static void ExecuteStart(IServiceProvider provider)
		{
			OnStart?.Invoke(null, provider);
		}

		public static void ExecuteRun()
		{
			OnRun?.Invoke(null, EventArgs.Empty);
		}

		public static void ExecuteExit()
		{
			OnExit?.Invoke(null, EventArgs.Empty);
		}
	}
}
