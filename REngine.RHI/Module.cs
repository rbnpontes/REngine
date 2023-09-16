using REngine.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public static class RHIModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry.Add<GraphicsSettings>();
		}
	}
}
