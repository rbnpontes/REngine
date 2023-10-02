using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class CommandBufferImpl : NativeObject, ICommandBuffer
	{
		private readonly bool pIsDeferred;
		public CommandBufferImpl(IntPtr handle, bool isDeferred) : base(handle)
		{
			pIsDeferred = isDeferred;
		}

		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer Copy(CopyTextureInfo copyInfo)
		{
			throw new NotImplementedException();
		}


		public ICommandBuffer Draw(DrawArgs args)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer Draw(DrawIndexedArgs args)
		{
			throw new NotImplementedException();
		}

		public Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetPipeline(IPipelineState pipelineState)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetVertexBuffer(IBuffer buffer)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers, ulong[] offsets)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer Unmap(IBuffer buffer, MapType mapType)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data)
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
		{
			throw new NotImplementedException();
		}

		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data)
		{
			throw new NotImplementedException();
		}
	}
}
