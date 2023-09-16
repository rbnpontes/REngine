using Diligent;
using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class SwapChainImpl : ISwapChain
	{
		private Diligent.ISwapChain? pSwapChain;
		private RHI.SwapChainDesc pDesc = new RHI.SwapChainDesc();
		private Dictionary<Diligent.ITextureView, ITextureView> pColorBuffers;
		private SwapChainAdapter pAdapter = new SwapChainAdapter();

		public event EventHandler<SwapChainResizeEventArgs>? OnResize;
		public event EventHandler? OnPresent;
		public event EventHandler? OnDispose;

		public SwapChainDesc Desc
		{
			get
			{
				if (pSwapChain is null)
					throw new ObjectDisposedException("Can´t get SwapChain desc, SwapChain is already disposed.");

				Diligent.SwapChainDesc nativeDesc = pSwapChain.GetDesc();
				pAdapter.Fill(ref nativeDesc, out pDesc);
				return pDesc;
			}
		}

		public ITextureView ColorBuffer { get; private set; }

		public ITextureView? DepthBuffer { get; private set; }
		public SwapChainSize Size
		{
			get
			{
				if (pSwapChain is null)
					throw new ObjectDisposedException("Can´t get SwapChain desc, SwapChain is already disposed.");

				var desc = pSwapChain.GetDesc();
				return new SwapChainSize(desc.Width, desc.Height);
			}
			set
			{
				Resize(value.Width, value.Height);
			}
		}
		public SwapChainTransform Transform 
		{
			get => (SwapChainTransform)(pSwapChain?.GetDesc().PreTransform ?? SurfaceTransform.Optimal);
			set
			{
				if(pSwapChain is null)
					throw new ObjectDisposedException("Can´t get SwapChain desc, SwapChain is already disposed.");
				var desc = pSwapChain.GetDesc();
				Resize(desc.Width, desc.Height, value);
			} 
		}
		public uint BufferCount
		{
			get => pSwapChain?.GetDesc().BufferCount ?? 0;
		}
		public SwapChainImpl(Diligent.ISwapChain swapChain)
		{
			pSwapChain = swapChain;
			pColorBuffers = new Dictionary<Diligent.ITextureView, ITextureView>();

			ColorBuffer = AcquireNextBuffer();
			var depthBuffer = swapChain.GetDepthBufferDSV();
			if (depthBuffer != null)
				DepthBuffer = new TextureViewImpl(depthBuffer);
		}

		public void Dispose()
		{
			if(pSwapChain != null)
			{
				pSwapChain.Dispose();
				OnDispose?.Invoke(this, EventArgs.Empty);
			}

			pSwapChain = null;
		}

		public ISwapChain Present(bool vsync)
		{
			pSwapChain?.Present(vsync ? 1u : 0u);
			ColorBuffer = AcquireNextBuffer();

			OnPresent?.Invoke(this, EventArgs.Empty);
			
			return this;
		}

		public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			return Resize((uint)size.Width, (uint)size.Height, transform);
		}
		public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			pSwapChain?.Resize(width, height, (Diligent.SurfaceTransform)transform);
			pColorBuffers.Clear();

			ColorBuffer = AcquireNextBuffer();

			var newDepth = pSwapChain?.GetDepthBufferDSV();
			if (newDepth != null)
				DepthBuffer = new TextureViewImpl(newDepth);

			OnResize?.Invoke(this, 
				new SwapChainResizeEventArgs(
					new SwapChainSize(width, height),
					transform
					)
			);

			return this;
		}

		private ITextureView AcquireNextBuffer()
		{
			if (pSwapChain is null)
				throw new ObjectDisposedException("Can´t get SwapChain desc, SwapChain is already disposed.");

			var colorBuffer = pSwapChain.GetCurrentBackBufferRTV();
			ITextureView? texture;

			if (pColorBuffers.TryGetValue(colorBuffer, out texture))
				return texture;

			texture = new TextureViewImpl(colorBuffer);

			// This case must never happen, but if diligent returns color buffer reference
			// at each Present call, we must garantere that color buffers don't have count greater than
			// SwapChain buffer count.
			if (pColorBuffers.Count >= pSwapChain.GetDesc().BufferCount)
				pColorBuffers.Clear();
			pColorBuffers.Add(colorBuffer, texture);

			return texture;
		}
	}
}
