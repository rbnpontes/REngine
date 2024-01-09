using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame.Components
{
	internal class PongMenuButton(IServiceProvider provider) : Behavior(provider)
	{
		private readonly TextureSpriteEffect pEffect = TextureSpriteEffect.Build(provider);
		public ITexture Texture
		{
			get => pEffect.Texture;
			set => pEffect.Texture = value;
		}
		
		public IAudio? HoverAudio { get; set; }

		public Action? ClickAction { get; set; }

		private SpriteComponent? pSpriteComponent;
		private Transform2D? pTransform;

		public override void OnSetup()
		{
			if (Owner is null)
				return;

			pTransform ??= Owner.GetComponent<Transform2D>() ?? Owner.CreateComponent<Transform2D>();
			pSpriteComponent ??= Owner.GetComponent<SpriteComponent>() ?? Owner.CreateComponent<SpriteComponent>();

			pSpriteComponent.Color = Color.White;
			pTransform.Scale = PongVariables.MenuTextureSize;

			pSpriteComponent.Effect = pEffect;
			pSpriteComponent.MouseEnterEvent.Add(HandleMouseEnter);
			pSpriteComponent.MouseExitEvent.Add(HandleMouseExit);
			pSpriteComponent.ClickEvent.Add(HandleClick);
		}

		protected override void OnDispose()
		{
			if (pSpriteComponent is null)
				return;
			pSpriteComponent.MouseEnterEvent.Remove(HandleMouseEnter);
			pSpriteComponent.MouseExitEvent.Remove(HandleMouseExit);
			pSpriteComponent.ClickEvent.Remove(HandleClick);
		}

		private void HandleClick(object? sender)
		{
			ClickAction?.Invoke();
		}

		private void HandleMouseEnter(object? sender)
		{
			if(pTransform is null || HoverAudio is null) return;

			HoverAudio.Play(true);
			pTransform.Scale = PongVariables.MenuTextureSize * new Vector2(1.1f);
		}

		private void HandleMouseExit(object? sender)
		{
			if(pTransform != null)
				pTransform.Scale = PongVariables.MenuTextureSize;
		}
	}
}
