using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class GraphicsDriverImpl : IGraphicsDriver
	{
		public GraphicsBackend Backend { get; internal set; }

		public string DriverName { get; internal set; } = string.Empty;

		public IReadOnlyList<ICommandBuffer> Commands { get; internal set; } = new List<ICommandBuffer>();
		public ICommandBuffer ImmediateCommand
		{
			get
			{
				if (Commands.Count == 0)
					throw new Exception("Can´t get immediate command. Command lists is empty.");
				return Commands[0];
			}
		}

		internal IEngineFactory? EngineFactory = null;

		private IDevice? pDevice = null;
		public IDevice Device
		{
			get
			{
				if (pDevice == null)
					throw new NullReferenceException("Device Object has not been setted. Did you initialize graphics correctly?");
				return pDevice;
			}
			set { pDevice = value; }
		}

		public IServiceProvider ServiceProvider { get; private set; }

		public GraphicsDriverImpl(IServiceProvider provider)
		{
			ServiceProvider = provider;
		}

		public void Dispose()
		{
			foreach (var cmd in Commands)
				cmd.Dispose();
			Device.Dispose();
		}
	}
}
