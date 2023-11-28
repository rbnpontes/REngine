using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.Sandbox.States
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
