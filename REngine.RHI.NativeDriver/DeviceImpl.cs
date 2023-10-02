using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class DeviceImpl : NativeObject, IDevice
	{
		public DeviceImpl(IntPtr handle) : base(handle)
		{
		}

		public IBuffer CreateBuffer(in BufferDesc desc)
		{
			throw new NotImplementedException();
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : struct
		{
			throw new NotImplementedException();
		}

		public IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
		{
			throw new NotImplementedException();
		}

		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
		{
			throw new NotImplementedException();
		}

		public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
		{
			throw new NotImplementedException();
		}

		public IShader CreateShader(in ShaderCreateInfo createInfo)
		{
			throw new NotImplementedException();
		}

		public ITexture CreateTexture(in TextureDesc desc)
		{
			throw new NotImplementedException();
		}

		public ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
		{
			throw new NotImplementedException();
		}
	}
}
