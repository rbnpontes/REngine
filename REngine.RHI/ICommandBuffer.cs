using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct DrawArgs
	{
		public uint NumVertices;
		public uint NumInstances;
		public uint StartVertexLocation;
		public uint FirstInstanceLocation;

		public DrawArgs()
		{
			NumVertices = 0;
			NumInstances = 1;
			StartVertexLocation = 0;
			FirstInstanceLocation = 0;
		}
	}
	public struct DrawIndexedArgs
	{
		public uint NumIndices;
		public ValueType IndexType;
		public uint NumInstances;
		public uint FirstIndexLocation;
		public uint BaseVertex;
		public uint FirstInstanceLocation;

		public DrawIndexedArgs()
		{
			this = default(DrawIndexedArgs);
			IndexType = ValueType.UInt32;
			NumInstances = 1;
		}
	}

	public interface ICommandBuffer : IDisposable
	{
		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil);
		public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor);
		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil);
		public ICommandBuffer SetPipeline(IPipelineState pipelineState);
		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState);

		public ICommandBuffer SetVertexBuffer(IBuffer buffer);
		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers);
		public ICommandBuffer SetVertexBuffers(uint startSlot, IEnumerable<IBuffer> buffers, ulong[] offsets);
		public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0);

		public ICommandBuffer CommitBindings(IPipelineStateResourceBinding resourceBinding);
		public ICommandBuffer Draw(DrawArgs args);
		public ICommandBuffer Draw(DrawIndexedArgs args);

		public Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged;
		public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags);
		public ICommandBuffer Unmap(IBuffer buffer, MapType mapType);
	}
}
