using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct ComputeArgs
	{
		public uint ThreadGroupCountX;
		public uint ThreadGroupCountY;
		public uint ThreadGroupCountZ;

		public ComputeArgs(uint threadGroupCountX = 1, uint threadGroupCountY = 1, uint threadGroupCountZ = 1)
		{
			ThreadGroupCountX = threadGroupCountX;
			ThreadGroupCountY = threadGroupCountY;
			ThreadGroupCountZ = threadGroupCountZ;
		}
	}
}
