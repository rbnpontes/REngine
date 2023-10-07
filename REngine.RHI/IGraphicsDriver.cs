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
		Dedicated,
		Discrete
	}

	public class GraphicsAdapter
	{
		public uint Id { get; set; }
		public uint DeviceId { get; set; }
		public uint VendorId { get; set; }
		public string Name { get; set; } = string.Empty;
		public AdapterType AdapterType { get; set; }
	}

	public interface IGraphicsDriver : IDisposable
	{
		public GraphicsBackend Backend { get; }
		public string DriverName { get; }
		public IReadOnlyList<ICommandBuffer> Commands { get; }
		public ICommandBuffer ImmediateCommand { get; }
		public IDevice Device { get; }

		public ISwapChain CreateSwapchain(in SwapChainDesc desc, ref NativeWindow window);
	}
}
