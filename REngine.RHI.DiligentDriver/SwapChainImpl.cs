using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class SwapChainImpl : ISwapChain
	{
		private Diligent.ISwapChain? pSwapChain;
		public SwapChainDesc Desc => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public SwapChainImpl(Diligent.ISwapChain swapChain)
		{
			pSwapChain = swapChain;
		}

		public void Dispose()
		{
			pSwapChain?.Dispose();
			pSwapChain = null;
		}

		public void Present(bool vsync)
		{
			pSwapChain?.Present(vsync ? 1u : 0u);
		}
	}
}
