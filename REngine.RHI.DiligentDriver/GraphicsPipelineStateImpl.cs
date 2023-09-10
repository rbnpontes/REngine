using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class GraphicsPipelineStateImpl : IPipelineState, INativeObject
	{
		private Diligent.IPipelineState? pHandle;
		private IPipelineStateResourceBinding? pResourceBinding;

		public object? Handle { get => pHandle; }
		public bool IsDisposed { get => pHandle == null; }

		public GraphicsPipelineDesc Desc { get; private set; }
		public string Name { get => pHandle?.GetDesc().Name ?? string.Empty; }

		public GraphicsPipelineStateImpl(GraphicsPipelineDesc desc, Diligent.IPipelineState pipeline)
		{
			pHandle = pipeline;
			Desc = desc;
		}

		public void Dispose()
		{
			pResourceBinding?.Dispose();
			pHandle?.Dispose();
			pHandle = null;
		}

		public IPipelineStateResourceBinding GetResourceBinding()
		{
			if (pHandle is null)
				throw new ObjectDisposedException("Can´t return resource binding. IPipelineState has been already disposed");

			if (pResourceBinding is null)
				pResourceBinding = new PipelineResourceBindingImpl(pHandle.CreateShaderResourceBinding(false));
			return pResourceBinding;
		}
	}
}
