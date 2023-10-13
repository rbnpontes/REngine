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

		private readonly object pSync = new();

		public SwapChainDesc Desc 
		{ 
			get
			{
				SwapChainDescNative desc = new();
				SwapChainDesc result = new();
				lock (pSync)
				{
					rengine_swapchain_get_desc(Handle, ref desc);
					SwapChainDescNative.CopyTo(desc, ref result);
				}
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

		private TextureViewWrapper pColorBuffer;
		private TextureViewWrapper? pDepthBuffer;

		public ITextureView ColorBuffer 
		{
			get => pColorBuffer;
		}

		public ITextureView? DepthBuffer
		{
			get => pDepthBuffer;
		}

		public uint BufferCount => Desc.BufferCount;

		public event EventHandler<SwapChainResizeEventArgs>? OnResize;
		public event EventHandler? OnPresent;

		public SwapChainImpl(IntPtr handle) : base(handle)
		{
			IntPtr ptr = rengine_swapchain_get_depthbuffer(Handle);
			if(ptr != IntPtr.Zero)
				pDepthBuffer = new TextureViewWrapper(ptr);
			pColorBuffer = new TextureViewWrapper(rengine_swapchain_get_backbuffer(Handle));
		}

		public ISwapChain Present(bool vsync)
		{
			lock (pSync)
			{
				rengine_swapchain_present(Handle, vsync ? 1u : 0u);
				OnPresent?.Invoke(this, EventArgs.Empty);

				CollectBuffers();
			}
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

			lock(pSync)
			{
				rengine_swapchain_resize(Handle, width, height, (uint)transform);
				CollectBuffers();
			}

			OnResize?.Invoke(this, 
				new SwapChainResizeEventArgs(
					new SwapChainSize(width, height),
					transform
				)
			);
			return this;
		}

		private void CollectBuffers()
		{
			pColorBuffer.Handle = rengine_swapchain_get_backbuffer(Handle);
			if(pDepthBuffer != null)
				pDepthBuffer.Handle = rengine_swapchain_get_depthbuffer(Handle);
		}
	}
}
