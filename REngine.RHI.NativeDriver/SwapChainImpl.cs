using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class SwapChainImpl : NativeObject, ISwapChain
	{
		private readonly object pSync = new();

		private SwapChainDesc pDesc;

		public SwapChainDesc Desc => pDesc;

		public SwapChainSize Size { 
			get => pDesc.Size;
			set => Resize(value.Width, value.Height, Transform);
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

		private readonly TextureViewWrapper pColorBuffer;
		private readonly TextureViewWrapper? pDepthBuffer;

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
			GetObjectDesc(handle, out pDesc);
			TextureSize texSize = new(pDesc.Size.Width, pDesc.Size.Height);

			IntPtr ptr = rengine_swapchain_get_depthbuffer(Handle);
			if(ptr != IntPtr.Zero)
				pDepthBuffer = new TextureViewWrapper(ptr, texSize);
			pColorBuffer = new TextureViewWrapper(rengine_swapchain_get_backbuffer(Handle), texSize);
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
			var currSize = pDesc.Size;
			if (currSize.Width == width && currSize.Height == height)
				return this;

			width = Math.Max(width, 1);
			height = Math.Max(height, 1);

			lock(pSync)
			{
				rengine_swapchain_resize(Handle, width, height, (uint)transform);
				CollectBuffers();
			}

			UpdateSize(width, height);
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

		private void UpdateSize(uint width, uint height)
		{
			var texSize = new TextureSize(width, height);
			pDesc.Size = new SwapChainSize(width, height);

			pColorBuffer.Size = texSize;
			if(pDepthBuffer != null)
				pDepthBuffer.Size = texSize;
		}
		public static void GetObjectDesc(IntPtr handle, out SwapChainDesc output)
		{
			SwapChainDescNative desc = new();
			SwapChainDesc result = new();
			
			rengine_swapchain_get_desc(handle, ref desc);
			SwapChainDescNative.CopyTo(desc, ref result);

			output = result;
		}
	}
}
