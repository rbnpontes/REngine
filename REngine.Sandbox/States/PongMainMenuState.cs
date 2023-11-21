using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.Sandbox.Components;

namespace REngine.Sandbox.States
{
	internal class PongMainMenuState : IGameState
	{
		private readonly EntityManager pEntityManager;
		private readonly IWindow pMainWindow;

		private Transform2D? pMenuTransform;
		public string Name => nameof(PongMainMenuState);

		public PongMainMenuState(
			EntityManager entityMgr,
			IWindow mainWindow)
		{
			pEntityManager = entityMgr;
			pMainWindow = mainWindow;
		}

		public void OnStart()
		{
			if (PongVariables.BackgroundAudio != null)
			{
				PongVariables.BackgroundAudio.Volume = PongVariables.AudioVolume;
				PongVariables.BackgroundAudio.Loop = true;
				PongVariables.BackgroundAudio.Play();
			}

			var menu = pEntityManager.CreateEntity("menu");
			var rootTransform = menu.CreateComponent<Transform2D>();

			pMenuTransform = rootTransform;
			UpdateMenuPosition();

			var menuButton = pEntityManager.CreateEntity("menu:play");
			var transform = menuButton.CreateComponent<Transform2D>();
			var button = menuButton.CreateComponent<PongMenuButton>();

			button.HoverAudio = PongVariables.MenuItemAudio;
			button.TextureSlot = PongVariables.MenuPlayButtonSlot;
			button.ClickAction = ExecutePlay;

			pMenuTransform.AddChild(transform);

			menuButton = pEntityManager.CreateEntity("menu:exit");
			transform = menuButton.CreateComponent<Transform2D>();
			button = menuButton.CreateComponent<PongMenuButton>();

			button.HoverAudio = PongVariables.MenuItemAudio;
			button.TextureSlot = PongVariables.MenuExitButtonSlot;
			button.ClickAction = ExecuteStop;

			transform.Position = new Vector2(0, PongVariables.MenuTextureSize.Y + PongVariables.MenuButtonMargin);
			pMenuTransform.AddChild(transform);
		}

		public void OnUpdate()
		{
			UpdateMenuPosition();
		}

		public void OnExit()
		{
		}

		private void ExecutePlay()
		{

		}

		private void ExecuteStop()
		{
			pMainWindow.Close();
		}

		private void UpdateMenuPosition()
		{
			if (pMenuTransform is null)
				return;

			var contentSize = PongVariables.MenuTextureSize with { Y = PongVariables.MenuButtonMargin + PongVariables.MenuTextureSize.Y * 2 };
			pMenuTransform.Position = (pMainWindow.Size.ToVector2() * 0.5f) - contentSize * 0.5f;
		}
	}
}
