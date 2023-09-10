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

	public interface ICommandBuffer : IDisposable
	{
		public ICommandBuffer SetRTs(ITextureView[] rts, ITextureView depthStencil);
		public ICommandBuffer ClearRT(ITextureView renderTarget, in Color clearColor);
		public ICommandBuffer ClearDepth(ITextureView depthStencil, ClearDepthStencil clearFlags, float depth, byte stencil);
		public ICommandBuffer SetPipeline(IPipelineState pipelineState);
		public ICommandBuffer SetPipeline(IComputePipelineState pipelineState);

		public ICommandBuffer Draw(DrawArgs args);
	}
}
