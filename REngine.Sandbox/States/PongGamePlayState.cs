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
		private readonly Vector2 pInitialViewSize = mainWindow.Size.ToVector2();
		private readonly object pSync = new();

#if DEBUG
		private readonly List<RectangleF> pBallTrajectoryDbg = new();
#endif
		private Transform2D[] pBlocks = Array.Empty<Transform2D>();
		private Transform2D? pBar;
		private Transform2D? pBall;
		private Transform2D? pRoot;

		private Vector2 pBallPosition;

		public string Name => nameof(PongGamePlayState);

		public void OnStart()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 0;

			var rootEntity = entityManager.CreateEntity("root");
			pRoot = rootEntity.CreateComponent<Transform2D>();

			var barEntity = entityManager.CreateEntity("bar");
			pBar = barEntity.CreateComponent<Transform2D>();
			pBar.Scale = PongVariables.BarSize;
			pBar.Position = new Vector2(pInitialViewSize.X * 0.5f - (PongVariables.BarSize.X * 0.5f), pInitialViewSize.Y - PongVariables.BarSize.Y);

			var sprite = barEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;

			var ball = entityManager.CreateEntity("ball");
			pBall = ball.CreateComponent<Transform2D>();
			pBall.Scale = new Vector2(PongVariables.BallRadius, PongVariables.BallRadius);
			pBall.Position = new Vector2(pBar.Position.X, pBar.Position.Y - PongVariables.BallRadius);

			sprite = ball.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
			sprite.Effect = new RoundedEffect();

			pRoot.AddChild(pBall);
			pRoot.AddChild(pBar);

			if(PongVariables.EnableDebug)
				imguiSystem.OnGui += OnGui;
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

			float deltaTime = (float)engine.DeltaTime;
			var windowScale = pInitialViewSize / mainWindow.Size.ToVector2();
			var ballSize = windowScale * PongVariables.BallRadius;

			UpdateRoot(windowScale);
			UpdateBall(deltaTime);
			UpdateBar();

			ComputeBarCollision(ref windowScale, ref ballSize);
			ComputeScreenCollisions(ref windowScale, ref ballSize);
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

		private void ComputeScreenCollisions(ref Vector2 wndScale, ref Vector2 ballSize)
		{
			var size = mainWindow.Size;
			var velocity = PongVariables.BallVelocity;

#if DEBUG
			if(PongVariables.EnableDebug)
				pBallTrajectoryDbg.Add(new RectangleF(pBallPosition.X, pBallPosition.Y, ballSize.X, ballSize.Y));
#endif
			if (pBallPosition.X + ballSize.X >= size.Width)
			{
				pBallPosition.X = size.Width - ballSize.X;
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

			PongVariables.BallVelocity = velocity;
		}

		private void ComputeBarCollision(ref Vector2 wndScale, ref Vector2 ballSize)
		{
			var pos = new Vector2(input.MousePosition.X - PongVariables.BarSize.X * 0.5f,
				mainWindow.Size.Height - PongVariables.BarSize.Y);
			var size = (PongVariables.BarSize);

#if DEBUG
			if(PongVariables.EnableDebug)
				pBallTrajectoryDbg.Add(new RectangleF(pBallPosition.X, pBallPosition.Y, ballSize.X, ballSize.Y));
#endif
			if ((!(pBallPosition.X >= pos.X) || !(pBallPosition.X + ballSize.X <= pos.X + size.X)) ||
			    !(pBallPosition.Y + ballSize.Y >= pos.Y)) return;
			pBallPosition.Y = pos.Y - ballSize.Y;
			PongVariables.BallVelocity *= new Vector2(1, -1);
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

		private void UpdateRoot(in Vector2 scale)
		{
			if(pRoot != null)
				pRoot.Scale = scale;
		}
	}
}
