using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct SwapChainCreateInfo
	{
		public byte backend;
		public IntPtr factory;
		public IntPtr device;
		public IntPtr deviceContext;
		public IntPtr swapChainDesc;
		public IntPtr window;
	}
}
