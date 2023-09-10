using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class PipelineStateImpl : IPipelineState, INativeObject
	{
		private Diligent.IPipelineState? pHandle;

		public object? Handle => pHandle;
		public GraphicsPipelineDesc Desc { get; private set; }

		public string Name => Desc.Name;

		public bool IsDisposed => pHandle == null;

		public PipelineStateImpl(Diligent.IPipelineState? handle, GraphicsPipelineDesc desc)
		{
			pHandle = handle;
			Desc = desc;
		}

		public void Dispose()
		{
			pHandle?.Dispose();
			pHandle = null;
		}
	}
}
