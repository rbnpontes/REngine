using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Serialization;

namespace REngine.RHI.DiligentDriver
{
#if WINDOWS
	public class D3D12Settings
	{
		private uint[] pCpuDescriptorHeapAllocationSize = new uint[4] { 8192, 2048, 1024, 1024 };
		private uint[] pGPUDescriptorHeapSize = new uint[2] { 16384, 1024 };
		private uint[] pGPUDescriptorHeapDynamicSize = new uint[2] { 8192, 1024 };
		private uint[] pDynamicDescriptorAllocationChunkSize = new uint[2] { 256, 32 };
		private uint[] pQueryPoolSize = new uint[6]
		{
			0,
			128,
			128,
			512,
			128,
			256
		};

		public uint[] CPUDescriptorHeapAllocationSize
		{
			get => pCpuDescriptorHeapAllocationSize;
			set {
				if (value.Length != 4)
					throw new ArgumentException("Array must be fixed length of 4 items");
				pCpuDescriptorHeapAllocationSize = value;
			}
		}
		public uint[] GPUDescriptorHeapSize
		{
			get => pGPUDescriptorHeapSize;
			set
			{
				if (value.Length != 2)
					throw new ArgumentException("Array must be fixed length of 2 items");
				pGPUDescriptorHeapSize = value;
			}
		}
		public uint[] GPUDescriptorHeapDynamicSize
		{
			get => pGPUDescriptorHeapDynamicSize;
			set
			{
				if (value.Length != 2)
					throw new ArgumentException("Array must be fixed length of 2 items");
				pGPUDescriptorHeapDynamicSize = value;
			}
		}
		public uint[] DynamicDescriptorAllocationChunkSize
		{
			get => pDynamicDescriptorAllocationChunkSize;
			set
			{
				if (value.Length != 2)
					throw new ArgumentException("Array must be fixed length of 2 items");
				pDynamicDescriptorAllocationChunkSize = value;
			}
		}
		public uint DynamicHeapPageSize { get; set; } = 1 << 20;
		public uint NumDynamicHeapPagesToReserve { get; set; } = 1;
		public uint[] QueryPoolSizes
		{
			get => pQueryPoolSize;
			set
			{
				if (value.Length != 6)
					throw new ArgumentException($"Array must be fixed length of 6 items.");
				pQueryPoolSize = value;
			}
		}
	}
#endif
	
	public class VulkanSettings
	{
		public class DescriptorPoolSize
		{
			public uint Max { get; set; }
			public uint SepSm { get; set; }
			public uint CmbSm { get; set; }
			public uint SmpImg { get; set; }
			public uint StrImg { get; set; }
			public uint UB { get; set; }
			public uint SB { get; set; }
			public uint UTxB { get; set; }
			public uint StTxB { get; set; }
			public uint InptAtt { get; set; }
			public uint AccelSt { get; set; }
		}

		private uint[] pQueryPoolSizes = new uint[6]
		{
			0,
			128,
			128,
			512,
			128,
			256
		};

		public string[] InstanceLayerNames { get; set; } = Array.Empty<string>();
		public string[] InstanceExtensionNames { get; set; } = Array.Empty<string>();
		public string[] DeviceExtensionNames { get; set; } = Array.Empty<string>();
		public string[] IgnoreDebugMessageNames { get; set; } = new string[0];
		public DescriptorPoolSize MainDescriptorPoolSize { get; set; } = new DescriptorPoolSize
		{
			Max = 8192,
			SepSm = 1024,
			CmbSm = 8192,
			SmpImg = 8192,
			StrImg = 1024,
			UB = 4096,
			SB = 4096,
			UTxB = 1024,
			StTxB = 1024,
			InptAtt = 256,
			AccelSt = 256,
		};
		public DescriptorPoolSize DynamicDescriptorPoolSize { get; set; } = new DescriptorPoolSize
		{
			Max = 2048,
			SepSm = 256,
			CmbSm = 2048,
			SmpImg = 2048,
			StrImg = 256,
			UB = 1024,
			SB = 1024,
			UTxB = 256,
			StTxB = 256,
			InptAtt = 64,
			AccelSt = 64
		};
		public uint DeviceLocalMemoryPageSize { get; set; } = 16 << 20;
		public uint HostVisibleMemoryPageSize { get; set; } = 16 << 20;
		public uint DeviceLocalMemoryReserveSize { get; set; } = 256 << 20;
		public uint HostVisibleMemoryReserveSize { get; set; } = 256 << 20;
		public uint UploadHeapPageSize { get; set; } = 1 << 20;
		public uint DynamicHeapSize { get; set; } = 8 << 20;
		public uint DynamicHeapPageSize { get; set; } = 256 << 10;
		public uint[] QueryPoolSizes
		{
			get => pQueryPoolSizes;
			set
			{
				if(value.Length != 6)
					throw new ArgumentException($"Array must be fixed length of 6 items");
				pQueryPoolSizes = value;
			}
		}
	}

	public class DriverSettings
	{
#if WINDOWS
		public D3D12Settings D3D12 { get; set; } = new D3D12Settings();
#endif
		// ReSharper disable once IdentifierTypo
		public VulkanSettings Vulkan { get; set; } = new VulkanSettings();
		public uint AdapterId { get; set; } = uint.MaxValue;
#if DEBUG
		public bool EnableValidation { get; set; } = true;
#else
		public bool EnableValidation { get; set; } = false;
#endif
#if WINDOWS
		public GraphicsBackend Backend { get; set; } = GraphicsBackend.D3D11;
#elif ANDROID
		public GraphicsBackend Backend { get; set; } = GraphicsBackend.OpenGL;
#else
		public GraphicsBackend Backend { get; set; } = GraphicsBackend.Vulkan;
#endif

		public void Merge(DriverSettings settings)
		{
#if WINDOWS
			D3D12 = settings.D3D12;
#endif
			Vulkan = settings.Vulkan;
			AdapterId = settings.AdapterId;
			EnableValidation = settings.EnableValidation;
			Backend = settings.Backend;
		}
		public static DriverSettings FromStream(Stream stream)
		{
			DriverSettings? settings;
			using (var reader = new StreamReader(stream))
				settings = reader.ReadToEnd().FromJson<DriverSettings>();
			return settings ?? new DriverSettings();
		}
	}
}
