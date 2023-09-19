using REngine.Core.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public struct SpriteBatchInfo
	{
		public Vector2 Position;
		public Vector2 Size;
		public Image? Image;

		public SpriteBatchInfo()
		{
			Position = Vector2.Zero;
			Size = Vector2.One;
			Image = null;
		}
	}
	public interface ISpriteBatch
	{
		public IRenderFeature Feature { get; }
		public ISpriteBatch Begin();
		public ISpriteBatch Draw(SpriteBatchInfo spriteInfo);
		public ISpriteBatch End();
	}
}
