using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core.Resources;

namespace REngine.Sandbox.States
{
	internal static class PongVariables
	{
		public const byte MenuPlayButtonSlot = 1;
		public const byte MenuExitButtonSlot = 2;
		public const byte MenuRestartButtonSlot = 3;

		public const float MenuButtonMargin = 8;

		public const float BallRadius = 15;
		public const float BlocksPerWidth = 10;
		public const float BlocksPerHeight = 5;

		public const float BallMargin = 0;

		public static readonly Vector2 BarSize = new(150, 10);
		public static readonly Vector2 MenuTextureSize = new(275, 66);

		public static readonly Queue<IAsset> Assets2Dispose  = new();

		public static float AudioVolume { get; set; } = 80f;
		public static IAudio? BackgroundAudio { get; set; }
		public static IAudio? MenuItemAudio { get; set; }

		public static float Speed { get; set; } = 100f;
		public static Vector2 BallVelocity { get; set; } = Vector2.One;
		public static void Reset()
		{
			Speed = 1;
			BallVelocity = Vector2.One;
			BackgroundAudio?.Stop();
			MenuItemAudio?.Stop();

			BackgroundAudio = MenuItemAudio = null;

			while(Assets2Dispose.TryDequeue(out var asset))
				asset.Dispose();
		}
	}
}
