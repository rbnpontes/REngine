using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
		public ulong localMemory;
		public ulong hostVisibleMemory;
		public ulong unifiedMemory;
		public ulong maxMemoryAlloc;
		public byte unifiedMemoryCpuAccess;
		public uint memoryLessTextureBindFlags;

		public static void Fill(in GraphicsAdapterNative x, GraphicsAdapter output)
		{
			output.Id = x.id;
			output.DeviceId = x.deviceId;
			output.VendorId = x.vendorId;
			output.Name = string.Intern(Marshal.PtrToStringUTF8(x.name) ?? string.Empty);
			output.AdapterType = (AdapterType)x.adapterType;
			output.LocalMemory = x.localMemory;
			output.HostVisibleMemory = x.hostVisibleMemory;
			output.UnifiedMemory = x.unifiedMemory;
			output.MaxMemoryAlloc = x.maxMemoryAlloc;
			output.UnifiedMemoryCpuAccess = (CpuAccessFlags)x.unifiedMemoryCpuAccess;
			output.MemorylessTextureBindFlags = (BindFlags)x.memoryLessTextureBindFlags;
		}
	}
}
