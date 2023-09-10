using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface IDevice : IDisposable
	{
		public IShader CreateShader(in ShaderCreateInfo createInfo);
		public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc);
		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc);
	}
}
