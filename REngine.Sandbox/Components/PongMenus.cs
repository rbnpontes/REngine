using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RPI.Components;
using REngine.RPI.RenderGraph;
using REngine.Sandbox.States;

namespace REngine.Sandbox.Components
{
	internal abstract class BlurMenu(IServiceProvider provider) : PongMenuButtons(provider)
	{
		private readonly IVar pBlurVar = provider.Get<IVariableManager>().GetVar("@vars/pong/blur");
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
			sprite.TextureSlot = PongVariables.MenuBackgroundSlot;
			sprite.Color = Color.White;

			base.OnSetup();
		}

		protected override void OnUpdate(float deltaTime)
		{
			if (pBackground is null)
				return;
			pBackground.Scale = mMainWindow.Size.ToVector2();
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

		protected override void OnBuildButton(int buttonIdx, out Action action, out byte textureSlot)
		{
			switch (buttonIdx)
			{
				case 0:
					textureSlot = PongVariables.MenuResumeButtonSlot;
					action = () => ResumeAction?.Invoke();
					break;
				case 1:
					textureSlot = PongVariables.MenuRestartButtonSlot;
					action = () => RestartAction?.Invoke();
					break;
				case 2:
					textureSlot = PongVariables.MenuExitButtonSlot;
					action = () => ExitAction?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}
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

		protected override void OnBuildButton(int buttonIdx, out Action action, out byte textureSlot)
		{
			switch (buttonIdx)
			{
				case 0:
					textureSlot = PongVariables.MenuResumeButtonSlot;
					action = () => PlayAction?.Invoke();
					break;
				case 1:
					textureSlot = PongVariables.MenuExitButtonSlot;
					action = () => ExitAction?.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}
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

		protected override void OnBuildButton(int buttonIdx, out Action action, out byte textureSlot)
		{
			switch (buttonIdx)
			{
				case 0:
					action = ()=> RestartAction?.Invoke();
					textureSlot = PongVariables.MenuRestartButtonSlot;
					break;
				case 1:
					action = () => ExitAction?.Invoke();
					textureSlot = PongVariables.MenuExitButtonSlot;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buttonIdx));
			}
		}
	}
}
