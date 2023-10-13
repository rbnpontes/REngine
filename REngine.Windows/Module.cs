using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	public static class WindowsModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry
				.Add(
					(args) => ((ILoggerFactory)args[0]).Build<IWindowManager>(),
					new Type[] { typeof(ILoggerFactory) }
				)
				.Add<IWindowManager, WindowManager>();
		}
	}
}
