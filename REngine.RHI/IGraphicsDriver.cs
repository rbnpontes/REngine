using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public enum GraphicsBackend
	{
		Unknow,
		D3D11,
		D3D12,
		Metal,
		Vulkan,
		OpenGL,
		Software
	}
	public interface IGraphicsDriver
	{
		public GraphicsBackend Backend { get; }
		public string DriverName { get; }
		public IReadOnlyList<ICommandBuffer> Commands { get; }
		public ICommandBuffer ImmediateCommand { get; }
		public IDevice Device { get; }
	}
}
