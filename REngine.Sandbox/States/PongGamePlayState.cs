using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Logic;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;
using REngine.RPI.Components;
using REngine.RPI.SpriteEffects;

namespace REngine.Sandbox.States
{
	internal class PongGamePlayState(EntityManager entityManager,
			IWindow mainWindow,
			IInput input,
			GameStateManager gameStateManager,
			IEngine engine)
		: IGameState
	{
		private Transform2D[] blocks = Array.Empty<Transform2D>();
		private Transform2D? bar;
		private Transform2D? pBall;
		private Transform2D? root;

		private Vector2 initialViewSize = mainWindow.Size.ToVector2();
		private Vector2 pBallPosition = new();

		public string Name => nameof(PongGamePlayState);

		public void OnStart()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 0;

			var rootEntity = entityManager.CreateEntity("root");
			root = rootEntity.CreateComponent<Transform2D>();

			var barEntity = entityManager.CreateEntity("bar");
			bar = barEntity.CreateComponent<Transform2D>();
			bar.Scale = PongVariables.BarSize;
			bar.Position = new Vector2(initialViewSize.X * 0.5f - (PongVariables.BarSize.X * 0.5f), initialViewSize.Y - PongVariables.BarSize.Y);

			var sprite = barEntity.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;

			var ball = entityManager.CreateEntity("ball");
			pBall = ball.CreateComponent<Transform2D>();
			pBall.Scale = new Vector2(PongVariables.BallRadius, PongVariables.BallRadius);

			sprite = ball.CreateComponent<SpriteComponent>();
			sprite.Color = Color.White;
			sprite.Effect = new RoundedEffect();

			root.AddChild(pBall);
			root.AddChild(bar);
		}

		public void OnUpdate()
		{
			var windowScale = initialViewSize / mainWindow.Size.ToVector2();

			ComputeScreenCollisions(ref windowScale);
			UpdateBar();
			UpdateBall();
			UpdateRoot(windowScale);
		}

		public void OnExit()
		{
			if (PongVariables.BackgroundAudio != null)
				PongVariables.BackgroundAudio.Pitch = 1;

			entityManager.DestroyAll();
		}

		private void ComputeScreenCollisions(ref Vector2 wndScale)
		{
			var size = mainWindow.Size;
			var ballSize = wndScale * PongVariables.BallRadius;
			var velocity = PongVariables.BallVelocity;

			if (pBallPosition.X + ballSize.X >= size.Width)
			{
				pBallPosition.X = size.Width - ballSize.X;
				velocity.X *= -1;
			}
			else if (pBallPosition.X < 0)
			{
				pBallPosition.X = ballSize.X;
				velocity.X *= -1;
			}

			if (pBallPosition.Y + ballSize.Y > size.Height)
			{
				pBallPosition.Y = size.Height - ballSize.Y;
				velocity.Y *= -1;
			}
			else if (pBallPosition.Y < 0)
			{
				pBallPosition.Y = ballSize.Y;
				velocity.Y *= -1;
			}

			PongVariables.BallVelocity = velocity;
		}

		private void UpdateBall()
		{
			if (pBall is null)
				return;
			pBallPosition += (PongVariables.BallVelocity * PongVariables.Speed) * (float)engine.DeltaTime;
			pBall.Position = pBallPosition;
		}
		private void UpdateBar()
		{
			if (bar is null)
				return;
			var pos = new Vector2(input.MousePosition.X - PongVariables.BarSize.X * 0.5f,
				mainWindow.Size.Height - PongVariables.BarSize.Y);
			bar.Position = pos;
		}

		private void UpdateRoot(in Vector2 scale)
		{
			if(root != null)
				root.Scale = scale;
		}
	}
}
