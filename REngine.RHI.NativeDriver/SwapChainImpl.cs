using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class SwapChainImpl : NativeObject, ISwapChain
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_swapchain_get_desc(IntPtr swapChain, ref SwapChainDescNative desc);
		[DllImport(Constants.Lib)]
		static extern void rengine_swapchain_present(IntPtr swapChain, uint sync);
		[DllImport(Constants.Lib)]
		static extern void rengine_swapchain_resize(IntPtr swapChain, uint width, uint height, uint transform);

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
			rengine_swapchain_present(Handle, vsync ? 1u : 0u);
			OnPresent?.Invoke(this, EventArgs.Empty);
			return this;
		}

		public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			return Resize((uint)size.Width, (uint)size.Height, transform);
		}

		public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			rengine_swapchain_resize(Handle, width, height, (uint)transform);
			OnResize?.Invoke(this, 
				new SwapChainResizeEventArgs(
					new SwapChainSize(width, height),
					transform
				)
			);
			return this;
		}

		protected override void BeforeRelease()
		{
			OnDispose?.Invoke(this, EventArgs.Empty);
		}
	}
}
