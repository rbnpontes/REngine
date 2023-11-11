using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public interface IPipelineStateManager
	{
		public IPipelineState GetOrCreate(GraphicsPipelineDesc desc);
		public IComputePipelineState GetOrCreate(ComputePipelineDesc desc);
		public IPipelineState? FindGraphicsPipelineByHash(ulong hash);
		public IComputePipelineState? FindComputePipelineByHash(ulong hash);
	}
}
