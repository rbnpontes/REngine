using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface IPipelineState : IGPUObject
	{
		public GraphicsPipelineDesc Desc { get; }
		public IShaderResourceBinding GetResourceBinding();
		public IShaderResourceBinding CreateResourceBinding();
	}

	public interface IComputePipelineState : IGPUObject
	{
		public ComputePipelineDesc Desc { get; }
		public IShaderResourceBinding CreateResourceBinding();
	}
}
