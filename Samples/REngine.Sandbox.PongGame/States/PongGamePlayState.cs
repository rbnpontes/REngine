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
using REngine.Core.Resources;
using REngine.Core.Storage;
using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Components;
// using REngine.RPkI.SpriteEffects;
using REngine.Sandbox.PongGame.Components;

namespace REngine.Sandbox.PongGame.States
{
	internal class PongGamePlayState(EntityManager entityManager,
			IWindow mainWindow,
			IInput input,
			GameStateManager gameStateManager,
			IEngine engine,
			IImGuiSystem imGuiSystem,
			IAssetManager assetManager)
		: IGameState
	{
#if PROFILER
		private static readonly string sProfilerUpdateSignature = $"{nameof(PongGamePlayState)}.OnUpdate()";
#endif
		private readonly object pSync = new();
#if DEBUG
		private readonly List<RectangleF> pBallTrajectoryDbg = new();
#endif
		private Transform2D?[] pBlocks = Array.Empty<Transform2D?>();
		private Transform2D? pBar;
		private Transform2D? pBall;
		private Transform2D? pRoot;

		private TextComponent? pText;
		private Transform2D? pTextTransform;
		private PongPausedMenu? pGameMenu;

		private Vector2 pBallPosition;

		public string Name => nameof(PongGamePlayState);

		public void OnStart()
		{
			SetInitialState();

			var wndSize = mainWindow.Size;
			var rootEntity = entityManager.CreateEntity("root");
			pRoot = rootEntity.CreateComponent<Transform2D>();

			var barEntity = entityManager.CreateEntity("bar");
			pBar = barEntity.CreateComponent<Transform2D>();
			pBar.Scale = PongVariables.BarSize;
			pBar.Position = new Vector2(wndSize.Width * 0.5f - (PongVariables.BarSize.X * 0.5f), wndSize.Height - PongVariables.BarSize.Y);

			var sprite = barEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;

			var ball = entityManager.CreateEntity("ball");
			pBall = ball.CreateComponent<Transform2D>();
			pBall.Scale = new Vector2(PongVariables.BallRadius, PongVariables.BallRadius);
			pBall.Position = pBallPosition = new Vector2(pBar.Position.X + PongVariables.BarSize.X * 0.5f, pBar.Position.Y - (PongVariables.BallRadius + PongVariables.BarSize.Y));

			sprite = ball.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
			// sprite.Effect = new RoundedEffect(assetManager);

			pRoot.AddChild(pBall);
			pRoot.AddChild(pBar);

			var textEntity = entityManager.CreateEntity("score");
			var textTransform = textEntity.CreateComponent<Transform2D>();
			var text = textEntity.CreateComponent<TextComponent>();
			text.FontName = PongVariables.DefaultFont;
			text.TextSize = 16;
			textTransform.Position = new Vector2(wndSize.Width, wndSize.Height - PongVariables.BarSize.Y - 16);
			pTextTransform = textTransform;

			pText = text;
			pRoot.AddChild(textTransform);

			CreateBlocks(pRoot);

			pGameMenu = entityManager.CreateEntity("menu").CreateComponent<PongPausedMenu>();
			pGameMenu.ResumeAction = () => pGameMenu.Visible = PongVariables.MenuActive = false;
			pGameMenu.RestartAction = () => gameStateManager.Restart();
			pGameMenu.ExitAction = () => engine.Stop();

#if DEBUG
			if(PongVariables.EnableDebug)
				imGuiSystem.OnGui += OnGui;
#endif
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
			if (pGameMenu is null)
				return;
#if PROFILER
			using (Profiler.Instance.Begin(sProfilerUpdateSignature))
			{
#endif
				if (input.GetKeyPress(InputKey.Esc))
					pGameMenu.Visible = PongVariables.MenuActive = !PongVariables.MenuActive;

				if (PongVariables.MenuActive)
					return;
#if DEBUG
				if (input.GetKeyPress(InputKey.Space) && PongVariables.EnableDebug)
					PongVariables.GamePaused = !PongVariables.GamePaused;
				if (input.GetKeyPress(InputKey.P))
				{
					PongVariables.EnableDebug = !PongVariables.EnableDebug;
					if (PongVariables.EnableDebug)
						imGuiSystem.OnGui += OnGui;
					else
						imGuiSystem.OnGui -= OnGui;
				}
#endif

				if (PongVariables.GamePaused)
					return;

				var deltaTime = (float)engine.DeltaTime;

				UpdateBall(deltaTime);
				UpdateBar();
				UpdateScoreTextPosition();

				ComputeBarCollision();
				ComputeScreenCollisions();
				ComputeBlocksCollision();
#if PROFILER
			}
#endif
		}

		public void OnExit()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 1;
#if DEBUG
			if(PongVariables.EnableDebug)
				imGuiSystem.OnGui -= OnGui;
#endif
			entityManager.DestroyAll();
		}

		private void ComputeScreenCollisions()
		{
#if PROFILER
			using (Profiler.Instance.Begin())
			{
#endif
				var size = mainWindow.Size;
				var velocity = PongVariables.BallVelocity;

#if DEBUG
				if (PongVariables.EnableDebug)
					pBallTrajectoryDbg.Add(new RectangleF(pBallPosition.X, pBallPosition.Y, PongVariables.BallRadius,
						PongVariables.BallRadius));
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

				if (pBallPosition.Y + PongVariables.BallRadius > size.Height)
				{
					gameStateManager.SetState(PongStates.GameOverPlayState);
					return;
				}

				PongVariables.BallVelocity = velocity;
#if PROFILER
			}
#endif
		}

		private void ComputeBarCollision()
		{
#if PROFILER
			using (Profiler.Instance.Begin())
			{

#endif
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
#if PROFILER
			}
#endif
		}

		private void ComputeBlocksCollision()
		{
			Transform2D? collidedBlock = null;
#if PROFILER
			using (Profiler.Instance.Begin())
			{
#endif
				var ballRect = new RectangleF(pBallPosition.X, pBallPosition.Y, PongVariables.BallRadius, PongVariables.BallRadius);
				for (var i = 0; i < pBlocks.Length; ++i)
				{
					var block = pBlocks[i];
					if (block is null)
						continue;
					var bounds = block.Bounds;
					if(!bounds.IntersectsWith(ballRect))
						continue;
					collidedBlock = block;
					pBlocks[i] = null;
					break;
				}
#if PROFILER
			}
#endif
			if (collidedBlock is null)
				return;

			ComputeBlockCollision(collidedBlock);
		}

		private void ComputeBlockCollision(Transform2D collidedBlock)
		{
			using (Profiler.Instance.Begin())
			{
				PongVariables.BallVelocity *= new Vector2(1, -1);
				if (collidedBlock.Owner != null)
					collidedBlock.Owner.Enabled = false;
			}

			ComputeScore();
		}

		private void ComputeScore()
		{
#if PROFILER
			using (Profiler.Instance.Begin())
			{
#endif
				// Play is an expensive operation. We don't want frame lag
				PongVariables.BlockClickAudio?.Play(true);

				PongVariables.Score += PongVariables.ScorePerBlock;
				const int totalScore = (PongVariables.BlocksPerCol * PongVariables.BlocksPerRow) * PongVariables.ScorePerBlock;
				var progress = PongVariables.Score / (float)totalScore;

				PongVariables.Speed = PongVariables.InitialSpeed + (progress * PongVariables.MaxSpeed);
				
				if (PongVariables.BackgroundAudio != null)
					PongVariables.BackgroundAudio.Pitch = progress * PongVariables.MaxPitch;

				if(pText != null)
					pText.Text = $"Score: {PongVariables.Score}";
#if PROFILER
			}
#endif
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
		private void UpdateScoreTextPosition()
		{
			if (pTextTransform is null || pText is null)
				return;

			var bounds = pText.Bounds;
			var size = mainWindow.Size;
			pTextTransform.Position = new Vector2(
				size.Width - (bounds.Width + PongVariables.ScoreTextMargin),
				size.Height - (bounds.Height + PongVariables.ScoreTextMargin)
			);
		}

		private static void SetInitialState()
		{
			PongVariables.MenuActive = false;
			PongVariables.GamePaused = false;
			PongVariables.Speed = PongVariables.InitialSpeed;
			PongVariables.Score = 0;
			PongVariables.BallVelocity = new Vector2(1, -1);
			
			if (PongVariables.BackgroundAudio == null) return;
			
			PongVariables.BackgroundAudio.Pitch = 0.01f;
			PongVariables.BackgroundAudio.Offset = TimeSpan.Zero;
		}
	}
}
