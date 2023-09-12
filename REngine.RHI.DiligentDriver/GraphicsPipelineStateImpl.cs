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

		public event GPUObjectEvent OnDispose = new GPUObjectEvent((obj, e) => { });

		public object? Handle { get => pHandle; }
		public bool IsDisposed { get => pHandle == null; }

		public GraphicsPipelineDesc Desc { get; private set; }
		public string Name { get => pHandle?.GetDesc().Name ?? string.Empty; }

		public GraphicsPipelineStateImpl(GraphicsPipelineDesc desc, Diligent.IPipelineState pipeline)
		{
			pHandle = pipeline;
			Desc = desc;
			pipeline.SetUserData(new ObjectWrapper(this));
		}

		public void Dispose()
		{
			if(pHandle != null)
			{
				pResourceBinding?.Dispose();
				pHandle?.Dispose();
				OnDispose(this, new EventArgs());
			}
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
