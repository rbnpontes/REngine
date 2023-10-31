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

	public interface ISpriteInstancing
	{ 
		public int Length { get; }
		
		public Vector2 GetPosition(int idx);
		public Vector2 GetAnchor(int idx);
		public Vector2 GetSize(int idx);
		public float GetAngle(int idx);

		public ISpriteInstancing SetPosition(int idx, Vector2 position);
		public ISpriteInstancing SetAnchor(int idx, Vector2 anchor);
		public ISpriteInstancing SetSize(int idx, Vector2 size);
		public ISpriteInstancing SetAngle(int idx, float angle);
	}

	public interface ISpriteBatch
	{
		public event EventHandler? OnDraw;
		/// <summary>
		/// Indicates if SpriteBatch has finished your jobs
		/// This doest not affect performance, but if you want 
		/// wait your contents to be renderer, you can check this value
		/// in a update loop. Or you can use WaitTask method to wait in a Task
		/// </summary>
		public bool IsReady { get; }
		/// <summary>
		/// Returns SpriteBatch Render Feature
		/// Don´t dispose this object, SpriteBatch handles for you
		/// </summary>
		public IGraphicsRenderFeature Feature { get; }
		public ISpriteBatch SetTexture(byte slot, ITexture texture);
		public ISpriteBatch SetTexture(byte slot, Image image);
		public ISpriteInstancing GetInstancing(int length);

		public ISpriteBatch Draw(TextRendererBatch textBatch);
		public ISpriteBatch Draw(SpriteBatchInfo spriteInfo);
		public ISpriteBatch Draw(byte textureSlot, ISpriteInstancing instancingItem);
		public ISpriteBatch ClearTexture(byte slot);
		public ISpriteBatch ClearTextures();
	}
}
