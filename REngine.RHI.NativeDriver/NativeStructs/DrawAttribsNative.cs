using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct DrawAttribsNative
	{
		public uint numVertices;
		public byte flags;
		public uint numInstances;
		public uint startVertexLocation;
		public uint firstInstanceLocation;

		public static void Fill(in DrawArgs args, ref DrawAttribsNative output)
		{
			output.numVertices = args.NumVertices;
			output.flags = 0x01 | 0x02 | 0x04;
			output.numInstances = args.NumInstances;
			output.startVertexLocation = args.StartVertexLocation;
			output.firstInstanceLocation = args.FirstInstanceLocation;
		}
	}

	internal struct DrawIndexedAttribsNative
	{
		public uint numIndices;
		public byte indexType;
		public byte flags;
		public uint numInstances;
		public uint firstIndexLocation;
		public uint baseVertex;
		public uint firstInstanceLocation;

		public static void Fill(in DrawIndexedArgs args, ref DrawIndexedAttribsNative output)
		{
			output.numIndices = args.NumIndices;
			output.indexType = (byte)args.IndexType;
			output.flags = 0x01 | 0x02 | 0x04;
			output.numInstances = args.NumInstances;
			output.firstIndexLocation = args.FirstIndexLocation;
			output.baseVertex = args.BaseVertex;
			output.firstInstanceLocation = args.FirstInstanceLocation;
		}
	}
}
