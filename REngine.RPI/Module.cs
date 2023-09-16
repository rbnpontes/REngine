using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public static class RPIModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry
				.Add<RenderSettings>()
				.Add<RendererEvents>()
				.Add(
					(deps) => ((ILoggerFactory)deps[0]).Build<IRenderer>(),
					new Type[] { typeof(ILoggerFactory) }
				)
				.Add<IRenderer, RendererImpl>()
				.Add<BasicFeaturesFactory>();
		}
	}
}
