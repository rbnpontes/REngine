using System.Numerics;
using REngine.Core.Resources;
using REngine.RPI;

namespace REngine.Sandbox.PongGame.States
{
	internal static class PongVariables
	{
		public const byte MenuPlayButtonSlot = 1;
		public const byte MenuExitButtonSlot = 2;
		public const byte MenuRestartButtonSlot = 3;
		public const byte MenuResumeButtonSlot = 4;
		public const byte MenuBackgroundSlot = 5;

		public const string DefaultFont = "Fonts/Anonymous-Pro.ttf";

		public const float MenuButtonMargin = 8;

		public const float ScoreTextMargin = 8;

		public const float BallRadius = 15;
		public const int BlocksPerCol = 10;
		public const int BlocksPerRow = 5;
		public const float BlockHeight = 30;
		public const float BlocksMargin = 5;

		public const int ScorePerBlock = 10;
		public const float InitialSpeed = 0.1f;
		public const float MaxSpeed = 1.0f;
		public const float MaxPitch = 1.3f;

		public static readonly Vector2 BarSize = new(300, 10);
		public static readonly Vector2 MenuTextureSize = new(275, 66);
		public static readonly Vector2 MenuTextureHalfSize = MenuTextureSize * 0.5f;

		public static SpriteEffect? LogoEffect;
		public static SpriteEffect? PlayButtonEffect;
		public static SpriteEffect? ExitButtonEffect;
		public static SpriteEffect? RestartButtonEffect;
		public static SpriteEffect? ResumeButtonEffect;
		public static SpriteEffect? BackgroundEffect;
		
		public static float AudioVolume { get; set; } = 50f;
		public static IAudio? BackgroundAudio { get; set; }
		public static IAudio? MenuItemAudio { get; set; }
		public static IAudio? BlockClickAudio { get; set; }

		public static float Speed { get; set; } = InitialSpeed;
		public static Vector2 BallVelocity { get; set; } = new Vector2(1, -1);

		public static bool MenuActive { get; set; }
		public static bool GamePaused { get; set; } = false;
		public static bool EnableDebug { get; set; } = false;

		public static int Score { get; set; }
		public static void Reset()
		{
			AudioVolume = 80f;
			Speed = InitialSpeed;
			Score = 0;
			MenuActive = false;

			BallVelocity = Vector2.One;
			BackgroundAudio?.Stop();
			MenuItemAudio?.Stop();
			BlockClickAudio?.Stop();
			
			LogoEffect?.Dispose();
			PlayButtonEffect?.Dispose();
			ExitButtonEffect?.Dispose();
			RestartButtonEffect?.Dispose();
			ResumeButtonEffect?.Dispose();
			BackgroundEffect?.Dispose();
			
			LogoEffect =
			PlayButtonEffect = 
			ExitButtonEffect = 
			RestartButtonEffect = 
			ResumeButtonEffect = 
			BackgroundEffect = null;

			BackgroundAudio = MenuItemAudio = BlockClickAudio = null;
		}
	}
}
