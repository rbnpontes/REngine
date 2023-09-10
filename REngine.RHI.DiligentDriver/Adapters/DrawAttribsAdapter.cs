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
	}
}
