using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public struct UIntSize
	{
		public uint Width;
		public uint Height;

		public UIntSize(uint width = 0, uint height = 0)
		{
			Width = width;
			Height = height;
		}
	}
}
