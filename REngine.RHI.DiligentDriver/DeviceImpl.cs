using Diligent;
using REngine.Core.DependencyInjection;
using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class DeviceImpl : IDevice
	{
		private IRenderDevice? pDevice;
		private GraphicsDriverImpl pDriver;

		public DeviceImpl(GraphicsDriverImpl driver, IRenderDevice device)
		{
			pDevice = device;
			pDriver = driver;
		}

		public IBuffer CreateBuffer(in BufferDesc desc)
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateBuffer)));
			Diligent.BufferDesc nativeDesc;
			BufferAdapter adapter = new BufferAdapter();
			adapter.Fill(desc, out nativeDesc);

			return new BufferImpl(pDevice.CreateBuffer(nativeDesc));
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateBuffer)));
			Diligent.BufferDesc nativeDesc;
			BufferAdapter adapter = new BufferAdapter();
			adapter.Fill(desc, out nativeDesc);

			return new BufferImpl(pDevice.CreateBuffer(nativeDesc, values.ToArray()));
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateBuffer)));
			Diligent.BufferDesc nativeDesc;
			BufferAdapter adapter = new BufferAdapter();
			adapter.Fill(desc, out nativeDesc);

			return new BufferImpl(pDevice.CreateBuffer(nativeDesc, values));
		}
		
		public IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : struct
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateBuffer)));

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IBuffer buffer = CreateBuffer(desc, handle.AddrOfPinnedObject(), (ulong)Marshal.SizeOf<T>());
			handle.Free();

			return buffer;
		}

		public unsafe IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateBuffer)));
			Diligent.BufferDesc nativeDesc;
			BufferAdapter adapter = new BufferAdapter();
			adapter.Fill(desc, out nativeDesc);
			return new BufferImpl(pDevice.CreateBuffer(nativeDesc, new Diligent.BufferData
			{
				Data = data,
				DataSize = size
			}));
		}

		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
		{
			throw new NotImplementedException();
		}

		public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateGraphicsPipeline)));

			var adapter = new PipelineStateAdapter(
				pDriver.ServiceProvider.Get<GraphicsSettings>(),
				pDriver.Backend == GraphicsBackend.OpenGL
			);

			GraphicsPipelineStateCreateInfo ci;
			adapter.Fill(desc, out ci);

			var pipeline = pDevice.CreateGraphicsPipelineState(ci);
			return new GraphicsPipelineStateImpl(desc, pipeline);
		}

		public IShader CreateShader(in ShaderCreateInfo createInfo)
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateShader)));
			Diligent.ShaderCreateInfo shaderCI;
			var adapter = new ShaderAdapter();
			adapter.Fill(in createInfo, out shaderCI);
			Diligent.IShader shader = pDevice.CreateShader(shaderCI, out _);
			return new ShaderImpl(shader);
		}

		public ITexture CreateTexture(in TextureDesc desc)
		{
			return CreateTexture(desc, new ITextureData[0]);
		}

		public ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
		{
			if (pDevice is null)
				throw new ObjectDisposedException(GetExecDisposedError(nameof(CreateTexture)));

			Diligent.TextureDesc nativeDesc;
			Diligent.TextureData textureData;
			var adapter = new TextureAdapter();
			adapter.Fill(desc, out nativeDesc);
			adapter.Fill(subresources, out textureData);

			return new TextureImpl(
				pDevice.CreateTexture(nativeDesc, subresources.Count() > 0 ? textureData : null)
			);
		}

		public void Dispose()
		{
			pDevice?.Dispose();
			pDevice = null;
		}

		private string GetExecDisposedError(string resource)
		{
			return $"Can´t execute {resource}. Device has been disposed.";
		}
	}
}
