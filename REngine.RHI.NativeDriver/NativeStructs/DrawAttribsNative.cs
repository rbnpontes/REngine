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

		public static void Fill(in DrawArgs args, out DrawAttribsNative output)
		{
			output = new DrawAttribsNative
			{
				numVertices = args.NumVertices,
				flags = 0x01 | 0x02 | 0x04,
				numInstances = args.NumInstances,
				startVertexLocation = args.StartVertexLocation,
				firstInstanceLocation = args.FirstInstanceLocation
			};
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

		public static void Fill(in DrawIndexedArgs args, out DrawIndexedAttribsNative output)
		{
			output = new DrawIndexedAttribsNative
			{
				numIndices = args.NumIndices,
				indexType = (byte)args.IndexType,
				flags = 0x01 | 0x02 | 0x04,
				numInstances = args.NumInstances,
				firstIndexLocation = args.FirstIndexLocation,
				baseVertex = args.BaseVertex,
				firstInstanceLocation = args.FirstInstanceLocation
			};
		}
	}
}
