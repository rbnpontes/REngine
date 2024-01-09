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
using REngine.Game.Components;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame.Components
{
	internal abstract class PongMenuButtons(IServiceProvider provider)
		: Behavior(provider)
	{
		private readonly Dictionary<int, Action> pMenuActions = new();
		protected readonly IWindow mMainWindow = provider.Get<IWindow>();
		protected readonly EntityManager mEntityManager = provider.Get<EntityManager>();

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
				OnSetVisible(value);
				pVisible = value;
				foreach (var button in pButtons)
					button.Enabled = value;
			}
		}


		protected virtual void OnSetVisible(bool value){}
		public override void OnSetup()
		{
			if (Owner is null)
				return;

			Transform.ZIndex = 1;
			List<SpriteComponent> components = new();
			var buttonCount = OnGetButtonCount();
			for (var i = 0; i < buttonCount; ++i)
			{
				OnBuildButton(i, out var action, out var effect);
				components.Add(
					CreateComponent(effect, $"menu:button:{i}", out var id)
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
				transform.ZIndex = 1 + i;

				pBounds = pBounds.Merge(transform.Bounds);
			}

			Visible = false;
			base.OnSetup();
		}

		protected abstract int OnGetButtonCount();
		protected abstract void OnBuildButton(int buttonIdx, out Action action, out SpriteEffect effect);

		protected override void OnUpdate(float deltaTime)
		{
			if (!pVisible)
				return;

			var size = mMainWindow.Size.ToVector2() * 0.5f;
			Transform.Position = size - PongVariables.MenuTextureHalfSize with { Y = pBounds.Height * 0.5f };
			
			if (pSelectedSprite == -1)
				return;

			pMenuActions.TryGetValue(pSelectedSprite, out var action);
			pSelectedSprite = -1;
			action?.Invoke();
		}

		private SpriteComponent CreateComponent(SpriteEffect effect, string name, out int id)
		{
			var spriteEntity = mEntityManager.CreateEntity(name);
			var transform = spriteEntity.CreateComponent<Transform2D>();
			var sprite = spriteEntity.CreateComponent<SpriteComponent>();

			sprite.Color = Color.White;
			sprite.Effect = effect;
			transform.Scale = PongVariables.MenuTextureSize;
			transform.ZIndex = spriteEntity.Id;

			sprite.MouseEnterEvent.Add(HandleMsEnter);
			sprite.MouseExitEvent.Add(HandleMsExit);
			sprite.ClickEvent.Add(HandleClick);

			id = spriteEntity.Id;

			Transform.AddChild(transform);
			return sprite;
		}

		private void HandleClick(object? sender)
		{
			if (sender is SpriteComponent sprite)
				pSelectedSprite = sprite.Owner?.Id ?? -1;
		}

		private void HandleMsEnter(object? sender)
		{
			if (sender is not SpriteComponent sprite)
				return;
			sprite.Transform.Scale = PongVariables.MenuTextureSize * new Vector2(1.1f);
			PongVariables.MenuItemAudio?.Play(true);
		}

		private void HandleMsExit(object? sender)
		{
			if (sender is not SpriteComponent sprite)
				return;
			sprite.Transform.Scale = PongVariables.MenuTextureSize;
		}
	}
}
