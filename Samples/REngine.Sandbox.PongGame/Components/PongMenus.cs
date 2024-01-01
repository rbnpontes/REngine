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
using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.RenderGraph;
using REngine.Sandbox.PongGame.Components;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame.Components
{
	internal abstract class BlurMenu(IServiceProvider provider) : PongMenuButtons(provider)
	{
		private readonly IVar pBlurVar = provider.Get<IVariableManager>().GetVar("@vars/pong/blur");
		private readonly GraphicsBackend pBackend = provider.Get<IGraphicsDriver>().Backend;
		private Transform2D? pBackground;

		protected override void OnSetVisible(bool value)
		{
			pBlurVar.Value = new Ref<bool>(value);
			if (pBackground?.Owner != null)
				pBackground.Owner.Enabled = value;
		}

		public override void OnSetup()
		{
			var bgEntity = mEntityManager.CreateEntity("menu:background");
			pBackground = bgEntity.CreateComponent<Transform2D>();
			var sprite = bgEntity.CreateComponent<SpriteComponent>();
			sprite.Effect = PongVariables.BackgroundEffect;
			sprite.Color = Color.White;
			if(pBackend == GraphicsBackend.OpenGL)
				sprite.Anchor = new Vector2(0, 1f);
			base.OnSetup();
		}

		protected override void OnUpdate(float deltaTime)
		{
			if (pBackground is null)
				return;
			pBackground.Scale = mMainWindow.Size.ToVector2();
			if (pBackend == GraphicsBackend.OpenGL)
				pBackground.Scale = new Vector2(pBackground.Scale.X, pBackground.Scale.Y * -1);
			base.OnUpdate(deltaTime);
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			pBackground?.Owner?.Dispose();
		}
	}
	internal class PongPausedMenu(IServiceProvider provider) : BlurMenu(provider)
	{
		public Action? ResumeAction { get; set; }
		public Action? RestartAction { get; set; }
		public Action? ExitAction { get; set; }
		protected override int OnGetButtonCount()
		{
			return 3;
		}

		protected override void OnBuildButton(int buttonIdx, out Action action, out SpriteEffect spriteEffect)
		{
			SpriteEffect? effect;
			switch (buttonIdx)
			{
				case 0:
					effect = PongVariables.ResumeButtonEffect	;
					action = () => ResumeAction?.Invoke();
					break;
				case 1:
					effect = PongVariables.RestartButtonEffect;
					action = () => RestartAction?.Invoke();
					break;
				case 2:
					effect = PongVariables.ExitButtonEffect;
					action = () => ExitAction?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}

			spriteEffect = effect ?? throw new NullReferenceException("Effect is null");
		}
	}
	internal class PongMainMenu(IServiceProvider provider) : PongMenuButtons(provider)
	{
		public Action? PlayAction { get; set; }
		public Action? ExitAction { get; set; }

		protected override int OnGetButtonCount()
		{
			return 2;
		}

		protected override void OnBuildButton(int buttonIdx, out Action action, out SpriteEffect spriteEffect)
		{
			SpriteEffect? effect;
			switch (buttonIdx)
			{
				case 0:
					effect = PongVariables.PlayButtonEffect;
					action = () => PlayAction?.Invoke();
					break;
				case 1:
					effect = PongVariables.ExitButtonEffect;
					action = () => ExitAction?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}

			spriteEffect = effect ?? throw new NullReferenceException("Effect is null");
		}
	}
	internal class PongGameOverMenu(IServiceProvider provider) : BlurMenu(provider)
	{
		public Action? RestartAction;
		public Action? ExitAction;
		protected override int OnGetButtonCount()
		{
			return 2;
		}

		protected override void OnBuildButton(int buttonIdx, out Action action, out SpriteEffect spriteEffect)
		{
			SpriteEffect? effect;
			switch (buttonIdx)
			{
				case 0:
					action = ()=> RestartAction?.Invoke();
					effect = PongVariables.RestartButtonEffect;
					break;
				case 1:
					action = () => ExitAction?.Invoke();
					effect = PongVariables.ExitButtonEffect;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}

			spriteEffect = effect ?? throw new NullReferenceException("Effect is null");
		}
	}
}
