using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.Core.Serialization;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public abstract class BaseSpriteComponent<T> : Component
	{
		protected readonly ISpriteBatch mSpriteBatch;
		protected readonly Transform2DSystem mTransformSystem;

		protected Transform2D? mTransform;
		[SerializationIgnore]
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

	// TODO: refactor this shit
	public sealed class SpriteComponent : BaseSpriteComponent<SpriteComponent>
	{
		private readonly IInput pInput;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly Action<IExecutionPipeline> pRenderBegin;

		private Transform2DSnapshot pLastSnapshot;

		public byte TextureSlot { get; set; } = byte.MaxValue;
		[SerializationIgnore]
		public BasicSpriteEffect? Effect { get; set; }
		public Vector2 Anchor { get; set; }
		public Vector2 Offset { get; set; }
		public Color Color { get; set; } = Color.White;

		public event EventHandler<EventArgs>? OnMouseOver;
		public event EventHandler<EventArgs>? OnMouseOut;
		public event EventHandler<EventArgs>? OnClick;

		public SpriteComponent(
			IServiceProvider provider,
			IInput input,
			IExecutionPipeline execPipeline
		) : base(provider)
		{
			pInput = input;
			pExecutionPipeline = execPipeline;

			pExecutionPipeline.AddEvent(DefaultEvents.RenderBeginId, pRenderBegin = OnRenderBegin);
		}

		private SpriteBatchInfo pBatchInfo = new();
		protected override void OnDraw(ISpriteBatch spriteBatch)
		{
			pBatchInfo.Position = pLastSnapshot.WorldPosition;
			pBatchInfo.Angle = pLastSnapshot.WorldRotation;
			pBatchInfo.Size = pLastSnapshot.Scale;
			pBatchInfo.Anchor = Anchor;
			pBatchInfo.Color = Color;
			pBatchInfo.Offset = Offset;
			pBatchInfo.TextureSlot = TextureSlot;
			pBatchInfo.Effect = Effect;

			spriteBatch.Draw(pBatchInfo);

			ComputeMouseInput();
		}

		private void OnRenderBegin(IExecutionPipeline _)
		{
			if (IsDisposed)
				return;

			Transform.GetSnapshot(out pLastSnapshot);
		}

		private bool pIsOver;
		private bool pIsOut;
		private void ComputeMouseInput()
		{
			var pos = Transform.WorldPosition;
			var scale = Transform.Scale;
			var mousePos = pInput.MousePosition;
			var bounds = new RectangleF(pos.X, pos.Y, scale.X, scale.Y);

			var isInside = bounds.Contains(mousePos.X, mousePos.Y);
			if (isInside)
			{
				pIsOut = false;
				ComputeMouseOverActions();
				return;
			}

			if (pIsOut)
				return;

			OnMouseOut?.Invoke(this, EventArgs.Empty);
			pIsOut = true;
			pIsOver = false;
		}

		private void ComputeMouseOverActions()
		{
			if (!pIsOver)
			{
				OnMouseOver?.Invoke(this, EventArgs.Empty);
				pIsOver = true;
			}

			if(pInput.GetMousePress(MouseKey.Left))
				OnClick?.Invoke(this, EventArgs.Empty);
		}
		protected override void OnDispose()
		{
			base.OnDispose();
			Effect?.Dispose();
			Effect = null;

			pExecutionPipeline.RemoveEvent(DefaultEvents.RenderBeginId, pRenderBegin);
		}
	}
}
