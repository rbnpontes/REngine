using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;

namespace REngine.RPI.Events
{
	internal static class EventsModule
	{
		public static void Setup(IServiceRegistry registry)
		{
			registry
				.Add<RPIEvents>()
				.Add<RendererEvents>()
				.Add<BufferManagerEvents>()
				.Add<ShaderManagerEvents>()
				.Add<PipelineStateManagerEvents>();
		}
	}
}
