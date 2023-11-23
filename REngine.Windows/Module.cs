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
	public sealed class WindowsModule : IModule
	{
		public void Setup(IServiceRegistry registry)
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
