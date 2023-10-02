using REngine.RHI.DiligentDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct DescriptorPoolSizeNative
	{
		public uint max;
		public uint sepSm;
		public uint cmbSm;
		public uint smpImg;
		public uint strImg;
		public uint ub;
		public uint sb;
		public uint utxb;
		public uint stTxB;
		public uint inptAtt;
		public uint accelSt;

		public DescriptorPoolSizeNative()
		{
			max =
			sepSm =
			cmbSm =
			smpImg =
			strImg =
			ub =
			sb =
			utxb =
			stTxB =
			inptAtt =
			accelSt = 0;
		}

		public static void From(VulkanSettings.DescriptorPoolSize desc, ref DescriptorPoolSizeNative output)
		{
			output.max = desc.Max;
			output.sepSm = desc.SepSm;
			output.cmbSm = desc.CmbSm;
			output.smpImg = desc.SmpImg;
			output.strImg = desc.StrImg;
			output.ub = desc.UB;
			output.sb = desc.SB;
			output.utxb = desc.UTxB;
			output.stTxB = desc.StTxB;
			output.inptAtt = desc.InptAtt;
			output.accelSt = desc.AccelSt;
		}
	}
	internal unsafe struct VulkanSettingsNative
	{
		public IntPtr* instanceLayerNames;
		public IntPtr* instanceExtensionNames;
		public IntPtr* deviceExtensionNames;
		public IntPtr* ignoreDebugMessageNames;

		public uint instanceLayerNamesCount;
		public uint instanceExtensionNamesCount;
		public uint deviceExtensionNamesCount;
		public uint ignoreDebugMessageNamesCount;

		public IntPtr mainDescriptorPoolSize;
		public IntPtr dynamicDescriptorPoolSize;

		public uint deviceLocalMemoryPageSize;
		public uint hostVisibleMemoryPageSize;
		public uint deviceLocalMemoryReserveSize;
		public uint hostVisibleMemoryReserveSize;
		public uint uploadHeapPageSize;
		public uint dynamicHeapSize;
		public uint dynamicHeapPageSize;

		public IntPtr queryPoolSizes;

		public VulkanSettingsNative()
		{
			instanceLayerNames =
			instanceExtensionNames =
			deviceExtensionNames =
			ignoreDebugMessageNames = (IntPtr*)IntPtr.Zero;


			instanceLayerNamesCount =
			instanceExtensionNamesCount =
			deviceExtensionNamesCount =
			ignoreDebugMessageNamesCount = 0;

			mainDescriptorPoolSize =
			dynamicDescriptorPoolSize = IntPtr.Zero;

			deviceLocalMemoryPageSize =
			hostVisibleMemoryPageSize =
			deviceLocalMemoryReserveSize =
			hostVisibleMemoryReserveSize =
			uploadHeapPageSize =
			dynamicHeapSize =
			dynamicHeapPageSize = 0;

			queryPoolSizes = IntPtr.Zero;
		}

		public static void From(VulkanSettings settings, ref VulkanSettingsNative output)
		{
			output.instanceLayerNamesCount = (uint)settings.InstanceLayerNames.Length;
			output.instanceExtensionNamesCount = (uint)settings.InstanceExtensionNames.Length;
			output.deviceExtensionNamesCount = (uint)settings.DeviceExtensionNames.Length;
			output.ignoreDebugMessageNamesCount = (uint)settings.IgnoreDebugMessageNames.Length;

			output.deviceLocalMemoryPageSize = settings.DeviceLocalMemoryPageSize;
			output.hostVisibleMemoryPageSize = settings.HostVisibleMemoryPageSize;
			output.deviceLocalMemoryReserveSize = settings.DeviceLocalMemoryReserveSize;
			output.hostVisibleMemoryReserveSize = settings.HostVisibleMemoryReserveSize;
			output.uploadHeapPageSize = settings.UploadHeapPageSize;
			output.dynamicHeapSize = settings.DynamicHeapSize;
			output.dynamicHeapPageSize = settings.DynamicHeapPageSize;
		}
	}
}
