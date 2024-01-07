using REngine.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;

namespace REngine.RHI
{
	public class RHIModule : IModule
	{
		public void Setup(IServiceRegistry registry)
		{
			registry
				.Add<GraphicsSettings>();
		}
	}
}
