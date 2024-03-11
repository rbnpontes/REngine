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
using REngine.RPI.RenderGraph;
using REngine.Sandbox.PongGame.Components;

namespace REngine.Sandbox.PongGame.States
{
	internal class PongMainMenuState(
		EntityManager entityMgr,
		IWindow mainWindow,
		GameStateManager gameStateManager,
		IEngine engine)
		: IGameState
	{
		public string Name => nameof(PongMainMenuState);

		public async Task OnStart()
		{
			await EngineGlobals.MainDispatcher.Yield();
			if (PongVariables.BackgroundAudio != null)
			{
				PongVariables.BackgroundAudio.Volume = PongVariables.AudioVolume;
				PongVariables.BackgroundAudio.Loop = true;
				PongVariables.BackgroundAudio.Pitch = 1;
				PongVariables.BackgroundAudio.Offset = TimeSpan.Zero;
				PongVariables.BackgroundAudio.Play();
			}

			var menu = entityMgr.CreateEntity("menu");
			var mainMenu = menu.CreateComponent<PongMainMenu>();
			mainMenu.Visible = true;
			mainMenu.PlayAction = ExecutePlay;
			mainMenu.ExitAction = ExecuteStop;
		}

		public void OnUpdate()
		{
		}

		public async Task OnExit()
		{
			await EngineGlobals.MainDispatcher.Yield();
			entityMgr.DestroyAll();
		}

		private void ExecutePlay()
		{
			gameStateManager.SetState(PongStates.PongGamePlayState);
		}

		private void ExecuteStop()
		{
			engine.Stop();
		}
	}
}
