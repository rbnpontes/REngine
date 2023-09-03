using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class CommandBufferImpl : ICommandBuffer
	{
		private IDeviceContext? pCtx;
		public CommandBufferImpl(IDeviceContext deviceContext)
		{
			pCtx = deviceContext;
		}

		public void Dispose()
		{
			pCtx?.Dispose();
			pCtx = null;
		}
	}
}
