using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public enum GraphicsBackend : byte
	{
		Unknow,
		D3D11,
		D3D12,
		Metal,
		Vulkan,
		OpenGL,
		Software
	}
	public enum AdapterType
	{
		Unknow=0,
		Software,
		Integrated,
		Discrete
	}

	public interface IGraphicsAdapter : IHashable
	{
		public uint Id { get; }
		public uint DeviceId { get; }
		public uint VendorId { get; }
		public string Name { get; }
		public AdapterType AdapterType { get; }
		public ulong LocalMemory { get; }
		public ulong HostVisibleMemory { get; }
		public ulong UnifiedMemory { get; }
		public ulong MaxMemoryAlloc { get; }
		public CpuAccessFlags UnifiedMemoryCpuAccess { get; }
		public BindFlags MemorylessTextureBindFlags { get; }
	}

	public interface IGraphicsDriver : IDisposable
	{
		public IGraphicsAdapter AdapterInfo { get; }
		public GraphicsBackend Backend { get; }
		public string DriverName { get; }
		public IReadOnlyList<ICommandBuffer> Commands { get; }
		public ICommandBuffer ImmediateCommand { get; }
		public IDevice Device { get; }

		public ISwapChain CreateSwapchain(in SwapChainDesc desc, ref NativeWindow window);
	}
}
