using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public static class CoreModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry
				.Add<ILoggerFactory, DebugLoggerFactory>()
				.Add<IInput, InputImpl>()
				.Add<IEngine, EngineImpl>()
				.Add<EngineEvents>();
		}
	}
}
