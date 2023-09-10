using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface IPipelineStateResourceBinding : IDisposable
	{
		public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource);
	}

	public interface IPipelineState : IGPUObject
	{
		public GraphicsPipelineDesc Desc { get; }
		public IPipelineStateResourceBinding GetResourceBinding();
	}

	public interface IComputePipelineState : IGPUObject
	{
		public ComputePipelineDesc Desc { get; }
		public IPipelineStateResourceBinding ComputeResourceBinding();
	}
}
