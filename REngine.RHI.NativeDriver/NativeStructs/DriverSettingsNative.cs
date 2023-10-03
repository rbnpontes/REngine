using REngine.RHI.DiligentDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct DriverSettingsNative
	{
#if WINDOWS
		public IntPtr d3d12;
#endif
		public IntPtr vulkan;
		public int enableValidation;
		public GraphicsBackend backend;

		public uint adapterId;
		public uint numDeferredCtx;
		public IntPtr messageCallback;

		public DriverSettingsNative()
		{
#if WINDOWS
			d3d12 = IntPtr.Zero;
#endif
			vulkan = IntPtr.Zero;
#if DEBUG
			enableValidation = 1;
#else
			enableValidation = 0;
#endif
			backend = GraphicsBackend.Unknow;

			adapterId = uint.MaxValue;
			numDeferredCtx = 0;
			messageCallback = IntPtr.Zero;
		}

		public static void From(DriverSettings settings, ref DriverSettingsNative output)
		{
			output.enableValidation = settings.EnableValidation ? 1 : 0;
			output.backend = settings.Backend;

			output.adapterId = settings.AdapterId;
		}
	}

	internal struct DriverNative
	{
		public IntPtr device;
		public IntPtr contexts;
		public IntPtr factory;

		public DriverNative()
		{
			device = contexts = factory = IntPtr.Zero;
		}
	}

	internal struct DriverResult
	{
		public IntPtr driver;
		public IntPtr swapChain;
		public IntPtr error;

		public DriverResult()
		{
			driver = swapChain = error = IntPtr.Zero;
		}
	}
}
