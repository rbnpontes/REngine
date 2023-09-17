using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Mathematics
{
	public static class MathExtensions
	{
		public static Color Lerp(this Color color, Color to, double time)
		{
			return Mathf.Lerp(color, to, time);
		}
		public static byte[] ToBytes(this Color color)
		{
			return new byte[]
			{
				color.A,
				color.R,
				color.G,
				color.B
			};
		}
	}
}
