using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Resources;

namespace REngine.RPI
{
	public interface IPipelineStateManager
	{
		/// <summary>
		/// Return current Pipeline State Cache
		/// This object is only available on D3D12 and Vulkan backends
		/// </summary>
		public IPipelineStateCache? PSCache { get; }
		public IPipelineState GetOrCreate(GraphicsPipelineDesc desc);
		public IComputePipelineState GetOrCreate(ComputePipelineDesc desc);
		public IComputePipelineState CreateComputeFromShader(ShaderAsset asset);
		public IComputePipelineState CreateComputeFromShader(IShader shader);
		public IPipelineState? FindGraphicsPipelineByHash(ulong hash);
		public IComputePipelineState? FindComputePipelineByHash(ulong hash);
	}
}
