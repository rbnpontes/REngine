using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
#if WINDOWS
	internal unsafe struct D3D12SettingsNative
	{
		public IntPtr cpuDescriptorHeapAllocationSize;
		public IntPtr gpuDescriptorHeapSize;
		public IntPtr gpuDescriptorHeapDynamicSize;
		public IntPtr dynamicDescriptorAllocationChunkSize;
		public IntPtr queryPoolSize;

		public uint dynamicHeapPageSize;
		public uint numDynamicHeapPagesToReserve;

		public D3D12SettingsNative()
		{
			cpuDescriptorHeapAllocationSize =
				gpuDescriptorHeapSize =
				gpuDescriptorHeapDynamicSize =
				dynamicDescriptorAllocationChunkSize =
				queryPoolSize = IntPtr.Zero;

			dynamicHeapPageSize = numDynamicHeapPagesToReserve = 0;
		}

		public static void From(D3D12Settings settings, ref D3D12SettingsNative output)
		{
			output.dynamicHeapPageSize = settings.DynamicHeapPageSize;
			output.numDynamicHeapPagesToReserve = settings.NumDynamicHeapPagesToReserve;
		}
	}
#endif
}
