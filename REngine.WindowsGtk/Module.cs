using REngine.Core;
using REngine.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	public static class WindowsModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry.Add<IWindowManager, WindowManager>();
		}
	}
}
