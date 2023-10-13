using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct Viewport
	{
		public Vector2 Position;
		public Vector2 Size;
		public float MinDepth;
		public float MaxDepth;

		public Rectangle Bounds;

		public Viewport() 
		{
			Position = new Vector2();
			Size = new Vector2();

			MinDepth = 0;
			MaxDepth = 1;

			Bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
		}
	}
}
