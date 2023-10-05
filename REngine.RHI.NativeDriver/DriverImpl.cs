using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class DriverImpl : IGraphicsDriver
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
			ObjectRegistry.ClearRegistry();

			pImmediateCmd = null;
			pDevice = null;
			NativeObject.rengine_object_releaseref(pFactory);
			pFactory = IntPtr.Zero;
			Commands = Array.Empty<ICommandBuffer>();

			pDisposed = true;

			GC.SuppressFinalize(this);
		}

		public unsafe ISwapChain CreateSwapchain(in SwapChainDesc desc, ref NativeWindow window)
		{
			AssertLife();
			if (Backend == GraphicsBackend.OpenGL)
				throw new NotSupportedException("Swapchain creation is not supported on OpenGL backend. SwapChain for OpenGL is provided at Driver creation");
			SwapChainDescNative swapChainDesc = new();
			SwapChainDescNative.From(desc, ref swapChainDesc);

			SwapChainCreateInfo ci = new();
			ci.backend = (byte)Backend;
			ci.factory = pFactory;
			ci.device = Device.Handle;
			ci.deviceContext = ImmediateCommand.Handle;
			ci.swapChainDesc = new IntPtr(Unsafe.AsPointer(ref swapChainDesc));
			ci.window = new IntPtr(Unsafe.AsPointer(ref window));

			ResultNative result = new();

			rengine_create_swapchain(ref ci, ref result);
			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Fatal Error while is creating SwapChain.");

			if (result.value == IntPtr.Zero)
				throw new NullReferenceException("SwapChain is null. Error has ocurred at SwapChain creation.");
			
			return new SwapChainImpl(result.value);
		}

		private void AssertLife()
		{
			if(pDisposed)
				throw new ObjectDisposedException("Driver has been disposed.");
		}
	}
}
