using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;
using REngine.RPI.SpriteEffects;

namespace REngine.Sandbox.States
{
	internal class PongGamePlayState(EntityManager entityManager,
			IWindow mainWindow,
			IInput input,
			GameStateManager gameStateManager,
			IEngine engine,
			IImGuiSystem imguiSystem)
		: IGameState
	{
		private readonly object pSync = new();

#if DEBUG
		private readonly List<RectangleF> pBallTrajectoryDbg = new();
#endif
		private Transform2D?[] pBlocks = Array.Empty<Transform2D?>();
		private Transform2D? pBar;
		private Transform2D? pBall;
		private Transform2D? pRoot;

		private TextComponent? pText;

		private Vector2 pBallPosition;

		public string Name => nameof(PongGamePlayState);

		public void OnStart()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 0.01f;

			var wndSize = mainWindow.Size;
			var rootEntity = entityManager.CreateEntity("root");
			pRoot = rootEntity.CreateComponent<Transform2D>();

			var barEntity = entityManager.CreateEntity("bar");
			pBar = barEntity.CreateComponent<Transform2D>();
			pBar.Scale = PongVariables.BarSize;
			pBar.Position = new Vector2(wndSize.Width * 0.5f - (PongVariables.BarSize.X * 0.5f), wndSize.Width - PongVariables.BarSize.Y);

			var sprite = barEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;

			var ball = entityManager.CreateEntity("ball");
			pBall = ball.CreateComponent<Transform2D>();
			pBall.Scale = new Vector2(PongVariables.BallRadius, PongVariables.BallRadius);
			pBall.Position = pBallPosition = new Vector2(pBar.Position.X, pBar.Position.Y - PongVariables.BallRadius);

			sprite = ball.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
			sprite.Effect = new RoundedEffect();

			pRoot.AddChild(pBall);
			pRoot.AddChild(pBar);

			var textEntity = entityManager.CreateEntity("score");
			var textTransform = textEntity.CreateComponent<Transform2D>();
			var text = textEntity.CreateComponent<TextComponent>();
			text.FontName = "Anonymous Pro.ttf";
			text.TextSize = 16;
			text.Text = "Score: 0";
			textTransform.Position = new Vector2(0, wndSize.Height - PongVariables.BarSize.Y - 16);

			pText = text;
			pRoot.AddChild(textTransform);

			CreateBlocks(pRoot);

			if(PongVariables.EnableDebug)
				imguiSystem.OnGui += OnGui;
		}

		private void CreateBlocks(Transform2D root)
		{
			var size = mainWindow.Size;
			var blockSize = new Vector2(
				((size.Width - (PongVariables.BlocksMargin * PongVariables.BlocksPerCol) - PongVariables.BlocksMargin) / PongVariables.BlocksPerCol),
				PongVariables.BlockHeight
			);

			pBlocks = new Transform2D[PongVariables.BlocksPerRow * PongVariables.BlocksPerCol];
			var nextIdx = 0;
			for (var row = 0; row < PongVariables.BlocksPerRow; row++)
			{
				for (var col = 0; col < PongVariables.BlocksPerCol; col++)
				{
					var pos = new Vector2(col * (blockSize.X + PongVariables.BlocksMargin), row * (blockSize.Y + PongVariables.BlocksMargin));
					pos.X += PongVariables.BlocksMargin;
					pos.Y += PongVariables.BlocksMargin;

					var blockEntity = entityManager.CreateEntity($"block({col}x{row})");
					var transform = blockEntity.CreateComponent<Transform2D>();
					var sprite = blockEntity.CreateComponent<SpriteComponent>();
					sprite.Color = Color.White;
					transform.Position = pos;
					transform.Scale = blockSize;

					root.AddChild(pBlocks[nextIdx++] = transform);
				}
			}
		}

#if DEBUG
		private void OnGui(object? sender, EventArgs e)
		{
			var drawList = ImGui.GetBackgroundDrawList();
			foreach (var rect in pBallTrajectoryDbg)
			{
				drawList.AddRect(
					new Vector2(rect.X, rect.Y),
					new Vector2(rect.Right, rect.Bottom),
					0xFF00FF00);
			}

			if(pBallTrajectoryDbg.Count > 500 && !PongVariables.GamePaused)
				pBallTrajectoryDbg.Clear();
		}
#endif

		public void OnUpdate()
		{
#if DEBUG
			if (input.GetKeyPress(InputKey.Space))
				PongVariables.GamePaused = !PongVariables.GamePaused;
			if (input.GetKeyPress(InputKey.P))
			{
				PongVariables.EnableDebug = !PongVariables.EnableDebug;
				if (PongVariables.EnableDebug)
					imguiSystem.OnGui += OnGui;
				else
					imguiSystem.OnGui -= OnGui;
			}
#endif

			if (PongVariables.GamePaused)
				return;

			var deltaTime = (float)engine.DeltaTime;

			UpdateBall(deltaTime);
			UpdateBar();

			ComputeBarCollision();
			ComputeScreenCollisions();
			ComputeBlocksCollision();
		}

		public void OnExit()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 1;
#if DEBUG
			if(PongVariables.EnableDebug)
				imguiSystem.OnGui -= OnGui;
#endif
			entityManager.DestroyAll();
		}

		private void ComputeScreenCollisions()
		{
			var size = mainWindow.Size;
			var velocity = PongVariables.BallVelocity;

#if DEBUG
			if(PongVariables.EnableDebug)
				pBallTrajectoryDbg.Add(new RectangleF(pBallPosition.X, pBallPosition.Y, PongVariables.BallRadius, PongVariables.BallRadius));
#endif
			if (pBallPosition.X + PongVariables.BallRadius >= size.Width)
			{
				pBallPosition.X = size.Width - PongVariables.BallRadius;
				velocity.X *= -1;
			}
			else if (pBallPosition.X <= 0)
			{
				pBallPosition.X = 0;
				velocity.X *= -1;
			}

			if (pBallPosition.Y < 0)
			{
				pBallPosition.Y = 0;
				velocity.Y *= -1;
			}

			if(pBallPosition.Y + PongVariables.BallRadius > size.Height)
				PongVariables.GamePaused = true;

			PongVariables.BallVelocity = velocity;
		}

		private void ComputeBarCollision()
		{
			var pos = new Vector2(input.MousePosition.X - PongVariables.BarSize.X * 0.5f,
				mainWindow.Size.Height - PongVariables.BarSize.Y);
			var size = (PongVariables.BarSize);

#if DEBUG
			if(PongVariables.EnableDebug)
				pBallTrajectoryDbg.Add(new RectangleF(pBallPosition.X, pBallPosition.Y, PongVariables.BallRadius, PongVariables.BallRadius));
#endif
			if ((!(pBallPosition.X >= pos.X) || !(pBallPosition.X + PongVariables.BallRadius <= pos.X + size.X)) ||
			    !(pBallPosition.Y + PongVariables.BallRadius >= pos.Y)) return;

			pBallPosition.Y = pos.Y - PongVariables.BallRadius;
			PongVariables.BallVelocity *= new Vector2(1, -1);
			PongVariables.BlockClickAudio?.Play(true);
		}

		private void ComputeBlocksCollision()
		{
			var ballRect = new RectangleF(pBallPosition.X, pBallPosition.Y, PongVariables.BallRadius, PongVariables.BallRadius);
			for (var i = 0; i < pBlocks.Length; ++i)
			{
				var block = pBlocks[i];
				if (block is null)
					continue;
				var bounds = block.Bounds;
				
				if(!bounds.IntersectsWith(ballRect))
					continue;

				ComputeScore();
				
				PongVariables.BallVelocity *= new Vector2(1, -1);
				if (block.Owner != null)
					block.Owner.Enabled = false;
				pBlocks[i] = null;
				return;
			}
		}

		private void ComputeScore()
		{
			PongVariables.BlockClickAudio?.Play(true);

			PongVariables.Score += PongVariables.ScorePerBlock;
			const int totalScore = (PongVariables.BlocksPerCol * PongVariables.BlocksPerRow) * PongVariables.ScorePerBlock;
			var progress = PongVariables.Score / (float)totalScore;

			PongVariables.Speed = PongVariables.InitialSpeed + (progress * PongVariables.MaxSpeed);
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = progress * PongVariables.MaxPitch;

			if(pText != null)
				pText.Text = $"Score: {PongVariables.Score}";
		}

		private void UpdateBall(float deltaTime)
		{
			if (pBall is null)
				return;
			pBallPosition += (PongVariables.BallVelocity * PongVariables.Speed) * deltaTime;
			pBall.Position = pBallPosition;
		}
		private void UpdateBar()
		{
			if (pBar is null)
				return;
			var pos = new Vector2(input.MousePosition.X - PongVariables.BarSize.X * 0.5f,
				mainWindow.Size.Height - PongVariables.BarSize.Y);
			pBar.Position = pos;
		}
	}
}
