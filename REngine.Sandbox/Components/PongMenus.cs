using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Sandbox.States;

namespace REngine.Sandbox.Components
{
	internal class PongPausedMenu(IServiceProvider provider) : PongMenuButtons(provider)
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

	internal class PongGameOverMenu(IServiceProvider provider) : PongMenuButtons(provider)
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
