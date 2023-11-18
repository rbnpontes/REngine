using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public abstract class BaseSpriteComponent<T> : Component
	{
		protected readonly ISpriteBatch mSpriteBatch;
		protected readonly Transform2DSystem mTransformSystem;

		protected Transform2D? mTransform;
		public Transform2D Transform
		{
			get
			{
				if (Owner is null)
					throw new NullReferenceException($"{nameof(T)} must be attached to an Entity");
				mTransform ??= Owner.GetComponent<Transform2D>();
				if (mTransform is not null) return mTransform;
				
				mTransform = mTransformSystem.CreateTransform();
				Owner.AddComponent(mTransform);

				return mTransform;
			}
		}

		internal BaseSpriteComponent(IServiceProvider provider)
		{
			mSpriteBatch = provider.Get<ISpriteBatch>();
			mTransformSystem = provider.Get<Transform2DSystem>();

			mSpriteBatch.OnDraw += HandleDraw;
		}

		private void HandleDraw(object? sender, EventArgs e)
		{
			if (!Enabled || Owner is null)
				return;
			OnDraw(mSpriteBatch);
		}
		
		protected abstract void OnDraw(ISpriteBatch spriteBatch);

		protected override void OnDispose()
		{
			mSpriteBatch.OnDraw -= HandleDraw;
		}
	}

	public sealed class SpriteComponent : BaseSpriteComponent<SpriteComponent>
	{
		public byte TextureSlot { get; set; } = byte.MaxValue;
		public BasicSpriteEffect? Effect { get; set; }
		public Vector2 Anchor { get; set; }
		public Vector2 Offset { get; set; }
		public Color Color { get; set; } = Color.White;
		public SpriteComponent(IServiceProvider provider) : base(provider)
		{
		}

		protected override void OnDraw(ISpriteBatch spriteBatch)
		{
			spriteBatch.Draw(new SpriteBatchInfo
			{
				Position = Transform.WorldPosition,
				Angle = Transform.WorldRotation,
				Size = Transform.Scale,
				Anchor = Anchor,
				Color = Color,
				Offset = Offset,
				TextureSlot = TextureSlot,
				Effect = Effect
			});
		}
	}
}
