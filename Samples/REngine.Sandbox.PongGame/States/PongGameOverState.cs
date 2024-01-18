using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core;
using REngine.Core.Logic;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.Game.Components;
using REngine.Sandbox.PongGame.Components;

namespace REngine.Sandbox.PongGame.States
{
	internal class PongGameOverState(
		IAssetManager assetManager,
		EntityManager entityManager,
		GameStateManager gameStateManager,
		IEngine engine)
		: IGameState
	{
		private PongGameOverMenu? pGameOverMenu;
		public string Name => nameof(PongGameOverState);

		public void OnStart()
		{
			if (PongVariables.BackgroundAudio != null)
			{
				PongVariables.BackgroundAudio.Offset = TimeSpan.Zero;
				PongVariables.BackgroundAudio.Loop = false;
				PongVariables.BackgroundAudio.Play(true);
			}

			pTime = 0;
			var fontAsset = assetManager.GetAsset<FontAsset>(PongVariables.DefaultFont);
			var menuEntity = entityManager.CreateEntity("menu:gameover");
			pGameOverMenu = menuEntity.CreateComponent<PongGameOverMenu>();
			pGameOverMenu.Visible = true;
			pGameOverMenu.ExitAction = () => engine.Stop();
			pGameOverMenu.RestartAction = () => gameStateManager.SetState(PongStates.PongGamePlayState);

			var textEntity = entityManager.CreateEntity("text:gameover");
			var transform = textEntity.CreateComponent<Transform2D>();
			var textComponent = textEntity.CreateComponent<TextComponent>();
			textComponent.Text = "Game Over";
			textComponent.Font = fontAsset.Font;

			transform.Position = new Vector2(10, -80);
			pGameOverMenu.Transform.AddChild(transform);

			textEntity = entityManager.CreateEntity("text:score");
			transform = textEntity.CreateComponent<Transform2D>();
			textComponent = textEntity.CreateComponent<TextComponent>();
			textComponent.Text = $"Score: {PongVariables.Score}";
			textComponent.Font = fontAsset.Font;

			transform.Position = new Vector2(10, -40);
			pGameOverMenu.Transform.AddChild(transform);
		}

		private float pTime;
		public void OnUpdate()
		{
			if (PongVariables.BackgroundAudio is null)
				return;
			pTime += 0.1f; // Decrease pitch speed
			pTime = (float)Math.Min(pTime, Math.PI);
			PongVariables.BackgroundAudio.Pitch = (float)(Math.Cos(pTime) + 1.0)*0.5f;
		}

		public void OnExit()
		{
			entityManager.DestroyAll();
		}
	}
}
