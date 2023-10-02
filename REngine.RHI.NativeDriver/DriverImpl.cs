using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class DriverImpl : IGraphicsDriver
	{
		private bool pDisposed = false;
		private ICommandBuffer? pImmediateCmd = null;
		private IDevice? pDevice = null;
		private IntPtr pFactory;

		public GraphicsBackend Backend { get; internal set; }

		public string DriverName => "REngine-NativeDriver";

		public IReadOnlyList<ICommandBuffer> Commands { get; private set; }

		public ICommandBuffer ImmediateCommand 
		{
			get
			{
				if (pDisposed)
					throw new ObjectDisposedException("Driver has been disposed");
				if (pImmediateCmd is null)
					throw new ObjectDisposedException("Immediate command buffer has been disposed.");
				return pImmediateCmd;
			}
		}

		public IDevice Device 
		{
			get
			{
				if (pDisposed)
					throw new ObjectDisposedException("Driver has been disposed");
				if (pDevice is null)
					throw new ObjectDisposedException("Device has been disposed");
				return pDevice;
			}
		}


		public DriverImpl(ICommandBuffer immediateCmd, ICommandBuffer[] commands, IDevice device, IntPtr factory)
		{
			Commands = commands;
			pImmediateCmd = immediateCmd;
			pDevice = device;
			pFactory = factory;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			foreach (var cmd in Commands)
				cmd.Dispose();
			pImmediateCmd?.Dispose();
			pDevice?.Dispose();

			pImmediateCmd = null;
			pDevice = null;
			NativeObject.rengine_object_releaseref(pFactory);
			pFactory = IntPtr.Zero;
			Commands = Array.Empty<ICommandBuffer>();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
