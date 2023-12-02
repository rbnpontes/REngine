using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;

namespace REngine.Android.Windows
{
	public sealed class WindowsModule : IModule
	{
		public void Setup(IServiceRegistry registry)
		{
			registry
				.Add<IWindowManager, WindowManager>();
		}
	}
}
