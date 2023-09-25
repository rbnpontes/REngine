using REngine.Core.Resources;
using REngine.RHI;
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
		public Vector2 Anchor;
		public Vector2 Offset;
		public Vector2 Size;
		public float Angle;
		public byte TextureSlot;
		public Color Color;

		public SpriteBatchInfo()
		{
			Position = Vector2.Zero;
			Anchor = Vector2.Zero;
			Offset = Vector2.Zero;
			Size = Vector2.One;
			Angle = 0;
			TextureSlot = byte.MaxValue;
			Color = Color.White;
		}
	}
	public struct SpriteInstancedBatchInfo
	{
		public Vector2 Position;
		public Vector2 Anchor;
		public Vector2 Size;
		public float Angle;

		public SpriteInstancedBatchInfo()
		{
			Position = Anchor = Vector2.Zero;
			Size = Vector2.One;
			Angle = 0;
		}
	}

	public interface ISpriteBatch
	{
		/// <summary>
		/// Returns SpriteBatch Render Feature
		/// Don´t dispose this object, SpriteBatch handles for you
		/// </summary>
		public IRenderFeature Feature { get; }
		public ISpriteBatch SetTexture(byte slot, ITexture texture);
		public ISpriteBatch SetTexture(byte slot, Image image);
		public ISpriteBatch Draw(SpriteBatchInfo spriteInfo);
		public ISpriteBatch Draw(byte textureSlot, IEnumerable<SpriteInstancedBatchInfo> spriteInstancedInfo);
		public ISpriteBatch ClearTexture(byte slot);
		public ISpriteBatch ClearTextures();
	}
}
