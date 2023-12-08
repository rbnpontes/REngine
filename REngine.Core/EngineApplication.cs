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
		public void OnSetLogger(ILogger logger);
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
		public IEngineStartup Stop();
	}
}
