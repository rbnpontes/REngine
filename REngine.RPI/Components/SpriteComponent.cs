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
using REngine.Core.Storage;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public abstract class BaseSpriteComponent<T> : Component
	{
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly Action<IExecutionPipeline> pBeginRenderAction;

		protected readonly ISpriteBatch mSpriteBatch;
		protected readonly Transform2DSystem mTransformSystem;

		private bool pSkipDraw;

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
			pExecutionPipeline = provider.Get<IExecutionPipeline>();
			mSpriteBatch = provider.Get<ISpriteBatch>();
			mTransformSystem = provider.Get<Transform2DSystem>();

			//mSpriteBatch.OnDraw += HandleDraw;
			pExecutionPipeline.AddEvent(DefaultEvents.RenderBeginId, pBeginRenderAction = BeginRender);
		}

		private void HandleDraw(object? sender, EventArgs e)
		{
			if (pSkipDraw || Owner is null)
				return;
			OnDraw(mSpriteBatch);
		}

		private void BeginRender(IExecutionPipeline _)
		{
			if (Owner is null)
				return;
			pSkipDraw = !Enabled;
			OnBeginRender();
		}
		
		protected abstract void OnDraw(ISpriteBatch spriteBatch);

		protected virtual void OnBeginRender(){}
		protected override void OnDispose()
		{
			//mSpriteBatch.OnDraw -= HandleDraw;
			pExecutionPipeline.RemoveEvent(DefaultEvents.RenderBeginId, pBeginRenderAction);
		}
	}

	// TODO: refactor this shit
	public sealed class SpriteComponent : BaseSpriteComponent<SpriteComponent>
	{
		private static readonly object sSync = new();
		[Flags]
		enum MotionEventFlags
		{
			None = 0,
			Over = 1 << 0,
			Out = 1 << 1,
			Click = 1 << 2,
		}

		private readonly IInput pInput;

		private Transform2DSnapshot pLastSnapshot;

		private MotionEventFlags pEventFlags;

		public byte TextureSlot { get; set; } = byte.MaxValue;
		[SerializationIgnore]
		// public BasicSpriteEffect? Effect { get; set; }
		public Vector2 Anchor { get; set; }
		public Vector2 Offset { get; set; }
		public Color Color { get; set; } = Color.White;

		public event EventHandler<EventArgs>? OnMouseOver;
		public event EventHandler<EventArgs>? OnMouseOut;
		public event EventHandler<EventArgs>? OnClick;

		public SpriteComponent(
			IServiceProvider provider,
			IInput input
		) : base(provider)
		{
			pInput = input;
		}

		//private SpriteBatchInfo pBatchInfo = new();
		protected override void OnDraw(ISpriteBatch spriteBatch)
		{
			lock (sSync)
			{
				// pBatchInfo.Position = pLastSnapshot.WorldPosition;
				// pBatchInfo.Angle = pLastSnapshot.WorldRotation;
				// pBatchInfo.Size = pLastSnapshot.Scale;
				// pBatchInfo.Anchor = Anchor;
				// pBatchInfo.Color = Color;
				// pBatchInfo.Offset = Offset;
				// pBatchInfo.TextureSlot = TextureSlot;
				// pBatchInfo.Effect = Effect;
				//
				// spriteBatch.Draw(pBatchInfo);

				ComputeMouseInput();
			}
		}

		protected override void OnBeginRender()
		{
			lock (sSync)
			{
				Transform.GetSnapshot(out pLastSnapshot);

				if (pEventFlags == MotionEventFlags.None)
					return;
				if((pEventFlags & MotionEventFlags.Over) != 0)
					OnMouseOver?.Invoke(this, EventArgs.Empty);
				if((pEventFlags & MotionEventFlags.Out) != 0)
					OnMouseOut?.Invoke(this, EventArgs.Empty);
				if((pEventFlags & MotionEventFlags.Click) != 0)
					OnClick?.Invoke(this, EventArgs.Empty);
				pEventFlags = 0;
			}
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

			pEventFlags |= MotionEventFlags.Out;
			pIsOut = true;
			pIsOver = false;
		}

		private void ComputeMouseOverActions()
		{
			if (!pIsOver)
			{
				pEventFlags |= MotionEventFlags.Over;
				pIsOver = true;
			}

			if (pInput.GetMousePress(MouseKey.Left))
				pEventFlags |= MotionEventFlags.Click;
		}
		protected override void OnDispose()
		{
			base.OnDispose();
			// Effect?.Dispose();
			// Effect = null;
		}
	}
}
