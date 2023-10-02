using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
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
		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_swapchain_get_backbuffer(IntPtr swapChain);
		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_swapchain_get_depthbuffer(IntPtr swapChain);

		public SwapChainDesc Desc 
		{ 
			get
			{
				SwapChainDescNative desc = new();
				SwapChainDesc result = new();
				rengine_swapchain_get_desc(Handle, ref desc);

				SwapChainDescNative.CopyTo(desc, ref result);
				return result;
			}
		}

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

		private Dictionary<IntPtr, ITextureView> pBuffers = new();
		private ITextureView? pColorBuffer;

		public ITextureView ColorBuffer 
		{ 
			get
			{
				return AcquireColorBuffer();
			}
		}

		public ITextureView? DepthBuffer { get; private set; }

		public uint BufferCount => throw new NotImplementedException();

		public event EventHandler<SwapChainResizeEventArgs>? OnResize;
		public event EventHandler? OnPresent;
		public event EventHandler? OnDispose;

		public SwapChainImpl(IntPtr handle) : base(handle)
		{
			DepthBuffer = AcquireDepthBuffer();
		}

		public ISwapChain Present(bool vsync)
		{
			rengine_swapchain_present(Handle, vsync ? 1u : 0u);
			OnPresent?.Invoke(this, EventArgs.Empty);

			pColorBuffer = null;
			return this;
		}

		public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			return Resize((uint)size.Width, (uint)size.Height, transform);
		}

		public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
		{
			var currSize = Size;
			if (currSize.Width == width && currSize.Height == height)
				return this;
			
			rengine_swapchain_resize(Handle, width, height, (uint)transform);
			OnResize?.Invoke(this, 
				new SwapChainResizeEventArgs(
					new SwapChainSize(width, height),
					transform
				)
			);
			
			DepthBuffer = AcquireDepthBuffer();
			pBuffers.Clear();
			pColorBuffer = null;
			return this;
		}

		private ITextureView AcquireColorBuffer()
		{
			ITextureView? colorBuffer = pColorBuffer;
			if(colorBuffer is null)
			{
				IntPtr colorBufferPtr = rengine_swapchain_get_backbuffer(Handle);
				if (colorBufferPtr == IntPtr.Zero)
					throw new NullReferenceException("Backbuffer is null");

				if(!pBuffers.TryGetValue(colorBufferPtr, out colorBuffer))
				{
					colorBuffer = new TextureViewImpl(colorBufferPtr);
					pBuffers.Add(colorBufferPtr, colorBuffer);
				}
			}

			return colorBuffer;
		}
		
		private ITextureView? AcquireDepthBuffer()
		{
			ITextureView? result = null;
			var depthBufferObj = ObjectRegistry.Acquire(rengine_swapchain_get_depthbuffer(Handle));
			if (depthBufferObj is ITextureView depthBuffer)
				result = depthBuffer;
			else
			{
				IntPtr depthPtr = rengine_swapchain_get_depthbuffer(Handle);
				if (depthPtr != IntPtr.Zero)
					result = new TextureViewImpl(depthPtr);
			}

			return result;
		}

		protected override void BeforeRelease()
		{
			OnDispose?.Invoke(this, EventArgs.Empty);
		}
	}
}
