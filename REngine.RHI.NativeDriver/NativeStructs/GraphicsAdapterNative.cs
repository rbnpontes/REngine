using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct GraphicsAdapterNative
	{
		public uint id;
		public uint deviceId;
		public uint vendorId;
		public IntPtr name;
		public byte adapterType;
	}
}
