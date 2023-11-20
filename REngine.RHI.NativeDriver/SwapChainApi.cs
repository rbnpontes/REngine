using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable IdentifierTypo

namespace REngine.RHI.NativeDriver
{
	internal partial class SwapChainImpl
	{
		[DllImport(Constants.Lib)]
		private static extern void rengine_swapchain_get_desc(IntPtr swapChain, ref SwapChainDescNative desc);
		[DllImport(Constants.Lib)]
		private static extern void rengine_swapchain_present(IntPtr swapChain, uint sync);
		[DllImport(Constants.Lib)]
		private static extern void rengine_swapchain_resize(IntPtr swapChain, uint width, uint height, uint transform);
		[DllImport(Constants.Lib)]
		private static extern IntPtr rengine_swapchain_get_backbuffer(IntPtr swapChain);
		[DllImport(Constants.Lib)]
		private static extern IntPtr rengine_swapchain_get_depthbuffer(IntPtr swapChain);
	}
}
