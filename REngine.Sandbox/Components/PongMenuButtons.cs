using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using REngine.RPI.Components;
using REngine.RPI.RenderGraph;
using REngine.Sandbox.States;

namespace REngine.Sandbox.Components
{
	internal abstract class PongMenuButtons(IServiceProvider provider)
		: Behavior(provider)
	{
		private readonly Dictionary<int, Action> pMenuActions = new();
		private readonly IWindow pMainWindow = provider.Get<IWindow>();
		private readonly EntityManager pEntityManager = provider.Get<EntityManager>();
		private readonly IVar pBlurVar = provider.Get<IVariableManager>().GetVar("@vars/pong/paused");

		private Transform2D? pBackground;
		private SpriteComponent[] pButtons = [];
		private Transform2D? pTransform;
		private int pSelectedSprite = -1;
		private RectangleF pBounds;

		private bool pVisible = true;

		public Transform2D Transform
		{
			get
			{
				if (Owner is null)
					throw new NullReferenceException(
						"Can´t retrieve transform. This component must be attached to an Entity");
				pTransform ??= Owner.GetComponent<Transform2D>() ?? Owner.CreateComponent<Transform2D>();
				return pTransform;
			}
		}

		public bool Visible
		{
			get => pVisible;
			set
			{
				if (pVisible == value)
					return;
				pBlurVar.Value = new Ref<bool>(!value);
				pVisible = value;
				if (pBackground?.Owner != null)
					pBackground.Owner.Enabled = value;
				foreach (var button in pButtons)
				{
					button.Enabled = value;
				}
			}
		}

		public override void OnSetup()
		{
			if (Owner is null)
				return;

			var bgEntity = pEntityManager.CreateEntity("menu:background");
			pBackground = bgEntity.CreateComponent<Transform2D>();
			var sprite = bgEntity.CreateComponent<SpriteComponent>();
			sprite.TextureSlot = PongVariables.MenuBackgroundSlot;
			sprite.Color = Color.White;
			

			List<SpriteComponent> components = new();
			var buttonCount = OnGetButtonCount();
			for (var i = 0; i < buttonCount; ++i)
			{
				OnBuildButton(i, out var action, out var texSlot);
				components.Add(
					CreateComponent(texSlot, $"menu:button:{i}", out var id)
				);

				pMenuActions[id] = action;
			}

			pButtons = [.. components];

			for (var i = 0; i < pButtons.Length; i++)
			{
				var pos = new Vector2(0,  (PongVariables.MenuTextureSize.Y + PongVariables.MenuButtonMargin) * i);
				pos.Y -= PongVariables.MenuButtonMargin;

				var transform = pButtons[i].Transform;
				transform.Position = pos;

				pBounds = pBounds.Merge(transform.Bounds);
			}

			Visible = false;
			base.OnSetup();
		}

		protected abstract int OnGetButtonCount();
		protected abstract void OnBuildButton(int buttonIdx, out Action action, out byte textureSlot);

		protected override void OnUpdate(float deltaTime)
		{
			if (pBackground is null)
				return;
			if (!pVisible)
				return;

			var size = pMainWindow.Size.ToVector2() * 0.5f;

			pBackground.Scale = pMainWindow.Size.ToVector2();
			Transform.Position = size - PongVariables.MenuTextureHalfSize with { Y = pBounds.Height * 0.5f };

			if (pSelectedSprite == -1)
				return;

			pMenuActions.TryGetValue(pSelectedSprite, out var action);
			pSelectedSprite = -1;
			action?.Invoke();
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			pBackground?.Owner?.Dispose();
			foreach (var button in pButtons)
			{
				button.OnClick -= HandleClick;
				button.OnMouseOver -= HandleMsOver;
				button.OnMouseOut -= HandleMsOut;
			}
		}

		private SpriteComponent CreateComponent(byte textureSlot, string name, out int id)
		{
			var spriteEntity = pEntityManager.CreateEntity(name);
			var transform = spriteEntity.CreateComponent<Transform2D>();
			var sprite = spriteEntity.CreateComponent<SpriteComponent>();

			sprite.Color = Color.White;
			sprite.TextureSlot = textureSlot;
			transform.Scale = PongVariables.MenuTextureSize;

			sprite.OnMouseOver += HandleMsOver;
			sprite.OnMouseOut += HandleMsOut;
			sprite.OnClick += HandleClick;

			id = spriteEntity.Id;

			Transform.AddChild(transform);
			return sprite;
		}

		private void HandleClick(object? sender, EventArgs e)
		{
			if (sender is SpriteComponent sprite)
				pSelectedSprite = sprite.Owner?.Id ?? -1;
		}

		private void HandleMsOver(object? sender, EventArgs e)
		{
			if (sender is not SpriteComponent sprite)
				return;
			sprite.Transform.Scale = PongVariables.MenuTextureSize * new Vector2(1.1f);
			PongVariables.MenuItemAudio?.Play(true);
		}

		private void HandleMsOut(object? sender, EventArgs e)
		{
			if (sender is not SpriteComponent sprite)
				return;
			sprite.Transform.Scale = PongVariables.MenuTextureSize;
		}
	}
}
