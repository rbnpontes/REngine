using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class DrawAttribsAdapter
	{
		public void Fill(ref DrawArgs args, out DrawAttribs attribs)
		{
			attribs = new DrawAttribs();
			attribs.NumVertices = args.NumVertices;
			attribs.Flags = DrawFlags.VerifyAll;
			attribs.NumInstances = args.NumInstances;
			attribs.StartVertexLocation = args.StartVertexLocation;
			attribs.FirstInstanceLocation = args.FirstInstanceLocation;
		}

		public void Fill(ref DrawIndexedArgs args, out DrawIndexedAttribs attribs)
		{
			attribs = new DrawIndexedAttribs();
			attribs.NumIndices = args.NumIndices;
			attribs.Flags = DrawFlags.VerifyAll;
			attribs.NumInstances = args.NumInstances;
			attribs.FirstInstanceLocation = args.FirstIndexLocation;
			attribs.BaseVertex = args.BaseVertex;
			attribs.FirstInstanceLocation = args.FirstInstanceLocation;
			attribs.IndexType = (Diligent.ValueType)args.IndexType;
		}
	}
}
