using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class SwapChainImpl : NativeObject, ISwapChain
	{
		public SwapChainDesc Desc { get; private set; }

		public SwapChainSize Size { 
			get => Desc.Size;
			set
			{
				Resize(value.Width, value.Height, Transform);
			}
		}
		public SwapChainTransform Transform
		{
			get => Desc.Transform;
			set
			{
				var size = Size;
				Resize(size.Width, size.Height, value);
			}
		}

		public ITextureView ColorBuffer => throw new NotImplementedException();

		public ITextureView? DepthBuffer => throw new NotImplementedException();

		public uint BufferCount => throw new NotImplementedException();

		public event EventHandler<SwapChainResizeEventArgs>? OnResize;
		public event EventHandler? OnPresent;
		public event EventHandler? OnDispose;

		public SwapChainImpl(IntPtr handle) : base(handle)
		{
		}

		public ISwapChain Present(bool vsync)
		{
			throw new NotImplementedException();
		}

		public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			throw new NotImplementedException();
		}

		public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			throw new NotImplementedException();
		}
	}
}
