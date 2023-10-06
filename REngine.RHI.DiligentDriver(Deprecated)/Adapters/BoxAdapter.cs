using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class BoxAdapter
	{
		public void Fill(Box box, out Diligent.Box output)
		{
			output = new Diligent.Box
			{
				MinX = box.MinX,
				MinY = box.MinY,
				MinZ = box.MinZ,

				MaxX = box.MaxX,
				MaxY = box.MaxY,
				MaxZ = box.MaxZ
			};
		}
	}
}
