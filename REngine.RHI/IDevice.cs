using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public interface IDevice : IDisposable, INativeObject
	{
		public IBuffer CreateBuffer(in BufferDesc desc);
		public IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged;
		public IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged;
		public IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : struct;
		public IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size);
		public IShader CreateShader(in ShaderCreateInfo createInfo);
		public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc);
		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc);

		public ITexture CreateTexture(in TextureDesc desc);
		public ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources);
	}
}
