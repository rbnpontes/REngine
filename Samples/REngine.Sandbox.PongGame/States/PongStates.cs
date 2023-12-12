using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;
using REngine.Sandbox.PongGame.States;

namespace REngine.Sandbox.PongGame.States
{
	public static class PongStates
	{
		public static readonly string SplashScreenState = nameof(SplashScreenState);
		public static readonly string PongMainMenuState = nameof(PongMainMenuState);
		public static readonly string LoadingPongState = nameof(LoadPongState);
		public static readonly string PongGamePlayState = nameof(PongGamePlayState);
		public static readonly string GameOverPlayState = nameof(PongGameOverState);
	}
}
