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
		private readonly Action<IExecutionPipeline> pBeginUpdate;
		private readonly Transform2DSystem pTransformSystem;

		private Transform2DSnapshot pLastSnapshot = new();
		private Transform2D? pTransform;
		[SerializationIgnore]
		public Transform2D Transform
		{
			get
			{
				if (Owner is null)
					throw new NullReferenceException($"{nameof(T)} must be attached to an Entity");
				if (pTransform is not null && !pTransform.IsDisposed) 
					return pTransform;

				pTransform = Owner.GetComponent<Transform2D>();
				if (pTransform is not null) 
					return pTransform;
				
				pTransform = pTransformSystem.CreateTransform();
				Owner.AddComponent(pTransform);
				return pTransform;
			}
		}
		
		internal BaseSpriteComponent(IServiceProvider provider)
		{
			pExecutionPipeline = provider.Get<IExecutionPipeline>();
			pTransformSystem = provider.Get<Transform2DSystem>();
			pBeginUpdate = BeginUpdate;
		}

		private void BeginUpdate(IExecutionPipeline pipeline)
		{
			if (pTransform is null || IsDisposed)
				return;
			
			pTransform.GetSnapshot(out var currSnapshot);
			if (!pLastSnapshot.Equals(currSnapshot))
			{
				pLastSnapshot = currSnapshot;
				OnChangeTransform();
			}
			
			OnUpdate();
		}

		public override void OnSetup()
		{
			BindEvents();
		}

		protected abstract void OnUpdate();
		protected abstract void OnChangeTransform();
		protected override void OnDispose()
		{
			pExecutionPipeline.RemoveEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
		}
		protected override void OnChangeVisibility(bool value)
		{
			BindEvents();
		}

		private void BindEvents()
		{
			if (Enabled)
				pExecutionPipeline.AddEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
			else
				pExecutionPipeline.RemoveEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
		}
	}
	
	public sealed class SpriteComponent(IServiceProvider provider) : BaseSpriteComponent<SpriteComponent>(provider)
	{
		[Flags]
		private enum MotionEventFlags
		{
			None = 0,
			Over = 1 << 0,
			Out = 1 << 1,
			Click = 1 << 2,
		}

		private readonly IInput pInput = provider.Get<IInput>();
		private readonly ISpriteBatch pSpriteBatch = provider.Get<ISpriteBatch>();

		private Sprite? pSprite;
        
		private MotionEventFlags pEventFlags;
		private bool pDirtyProps;
		

		private Vector2 pAnchor;
		private Color pColor = Color.White;
		private SpriteEffect? pEffect;
		
		[SerializationIgnore]
		public SpriteEffect? Effect
		{
			get => pEffect;
			set
			{
				pDirtyProps |= pEffect != value;
				pEffect = value;
			}
		}

		public Vector2 Anchor
		{
			get => pAnchor;
			set
			{
				pDirtyProps |= pAnchor != value;
				pAnchor = value;
			}
		}
		public Color Color
		{
			get => pColor;
			set
			{
				pDirtyProps |= pColor != value;
				pColor = value;
			}
		}

		public event EventHandler<EventArgs>? OnMouseOver;
		public event EventHandler<EventArgs>? OnMouseOut;
		public event EventHandler<EventArgs>? OnClick;

		public override void OnSetup()
		{
			base.OnSetup();
			pSprite ??= pSpriteBatch.CreateSprite();
			UpdateSpriteProps(pSprite);
			
			pInput.OnMousePressed += HandleMouseClick;
		}

		private void UpdateSpriteProps(Sprite sprite)
		{
			sprite.Lock();
			sprite.Anchor = pAnchor;
			sprite.Position = new Vector3(Transform.WorldPosition, Transform.ZIndex);
			sprite.Angle = Transform.WorldRotation;
			sprite.Size = Transform.Scale;
			if (pEffect is not null)
				sprite.Effect = pEffect;
			sprite.Color = pColor;
			sprite.Enabled = Enabled;
			sprite.Unlock();

			pDirtyProps = false;
		}
		
		protected override void OnUpdate()
		{
			if (pSprite is null)
				return;
			
			if(pDirtyProps)
				UpdateSpriteProps(pSprite);
			ComputeMouseInput();

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

		protected override void OnChangeVisibility(bool value)
		{
			base.OnChangeVisibility(value);
			if(pSprite is not null)
				UpdateSpriteProps(pSprite);
		}

		protected override void OnChangeTransform()
		{
			pDirtyProps = true;
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
			if (pIsOver) return;
			pEventFlags |= MotionEventFlags.Over;
			pIsOver = true;
		}
		
		private void HandleMouseClick(object? sender, InputMouseEventArgs e)
		{
			if (e.Key != MouseKey.Left)
				return;
			
			var msPos = pInput.MousePosition;
			var pos = Transform.WorldPosition;
			var scale = Transform.Scale;
			var bounds = new RectangleF(pos.X, pos.Y, scale.X, scale.Y);
			// Test if mouse is inside sprite and emit click
			if (bounds.Contains(msPos.X, msPos.Y))
				pEventFlags |= MotionEventFlags.Click;
		}
		
		protected override void OnDispose()
		{
			base.OnDispose();
			pSprite?.Dispose();
			pSprite = null;
			Effect = null;

			pInput.OnMousePressed -= HandleMouseClick;
		}
	}
}
