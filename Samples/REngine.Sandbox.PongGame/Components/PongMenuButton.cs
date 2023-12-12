using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.RPI.Components;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame.Components
{
	internal class PongMenuButton(IServiceProvider provider) : Behavior(provider)
	{
		public byte TextureSlot
		{
			get => pSpriteComponent?.TextureSlot ?? 0;
			set
			{
				if(pSpriteComponent != null)
					pSpriteComponent.TextureSlot = value;
			}
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

			pSpriteComponent.OnMouseOver += HandleMouseOver;
			pSpriteComponent.OnMouseOut += HandleMouseOut;
			pSpriteComponent.OnClick += HandleClick;
		}

		protected override void OnDispose()
		{
			if (pSpriteComponent is null)
				return;
			pSpriteComponent.OnMouseOver -= HandleMouseOver;
			pSpriteComponent.OnClick -= HandleClick;
		}

		private void HandleClick(object? sender, EventArgs e)
		{
			ClickAction?.Invoke();
		}

		private void HandleMouseOver(object? sender, EventArgs e)
		{
			if(pTransform is null || HoverAudio is null) return;

			HoverAudio.Play(true);
			pTransform.Scale = PongVariables.MenuTextureSize * new Vector2(1.1f);
		}

		private void HandleMouseOut(object? sender, EventArgs e)
		{
			if(pTransform != null)
			pTransform.Scale = PongVariables.MenuTextureSize;
		}
	}
}
