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
	}

	public interface IComputePipelineState : IGPUObject
	{
		public ComputePipelineDesc Desc { get; }
	}
}
