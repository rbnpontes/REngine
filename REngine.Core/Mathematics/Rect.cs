using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public struct Rect
	{
		public float Left;
		public float Top;
		public float Right;
		public float Bottom;
	}
	public unsafe struct IntRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public ref IntRect GetPinnableReference()
		{
			return ref this;
		}
	}
}
