using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class DriverImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_create_swapchain(ref SwapChainCreateInfo createInfo, ref ResultNative result);
	
	}
}
