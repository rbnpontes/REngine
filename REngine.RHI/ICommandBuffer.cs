using REngine.Core;
using REngine.Core.Mathematics;
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

	public class CopyTextureInfo
	{
		public ITexture? SrcTexture { get; set; }
		public uint SrcMipLevel { get; set; }
		public uint SrcSlice { get; set; }
		public Box? SrcBox { get; set; }
		public ITexture? DstTexture { get; set; }
		public uint DstMipLevel { get; set; }
		public uint DstSlice { get; set; }
		public uint DstX { get; set; }
		public uint DstY { get; set; }
		public uint DstZ { get; set; }

		public CopyTextureInfo()
		{
		}
		public CopyTextureInfo(ITexture srcTex, ITexture dstTex)
		{
			SrcTexture = srcTex;
			DstTexture = dstTex;
		}
	}

	public interface ICommandBuffer : IDisposable, INativeObject
	{
		public ICommandBuffer SetRT(ITextureView rt, ITextureView depthStencil);
		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView? depthStencil);
		public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor);
		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil);
		public ICommandBuffer SetPipeline(IPipelineState pipelineState);
		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState);

		public ICommandBuffer SetVertexBuffer(IBuffer buffer, bool reset = true);
		public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, bool reset = true);
		public ICommandBuffer SetVertexBuffers(uint startSlot, IBuffer[] buffers, ulong[] offsets, bool reset = true);
		public ICommandBuffer SetIndexBuffer(IBuffer buffer, ulong byteOffset = 0);

		public ICommandBuffer CommitBindings(IShaderResourceBinding resourceBinding);
		public ICommandBuffer Draw(DrawArgs args);
		public ICommandBuffer Draw(DrawIndexedArgs args);

		public ICommandBuffer Compute(ComputeArgs args);

		public ICommandBuffer SetBlendFactors(in Color color);
		public ICommandBuffer SetViewports(Viewport[] viewports, uint rtWidth, uint rtHeight);
		public ICommandBuffer SetViewport(Viewport viewports, uint rtWidth, uint rtHeight);
		public ICommandBuffer SetScissors(IntRect[] scissors, uint rtWidth, uint rtHeight);
		public ICommandBuffer SetScissor(IntRect scissor, uint rtWidth, uint rtHeight);

		public Span<T> Map<T>(IBuffer buffer, MapType mapType, MapFlags mapFlags) where T : unmanaged;
		public IntPtr Map(IBuffer buffer, MapType mapType, MapFlags mapFlags);
		public ICommandBuffer Unmap(IBuffer buffer, MapType mapType);

		public ICommandBuffer Copy(CopyTextureInfo copyInfo);

		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, T data) where T : unmanaged;
		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, byte[] data);
		public ICommandBuffer UpdateBuffer<T>(IBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged;
		public ICommandBuffer UpdateBuffer(IBuffer buffer, ulong offset, ulong size, IntPtr data);

#if DEBUG
		public ICommandBuffer BeginDebugGroup(string name, Color color);
		public ICommandBuffer EndDebugGroup();
		public ICommandBuffer InsertDebugLabel(string label, Color color);
#endif
	}
}
