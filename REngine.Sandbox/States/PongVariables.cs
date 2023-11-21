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
		public static readonly Vector2 MenuTextureSize = new(275, 66);

		public static readonly Queue<IAsset> Assets2Dispose  = new();

		public static float AudioVolume { get; set; } = 80f;
		public static IAudio? BackgroundAudio { get; set; }
		public static IAudio? MenuItemAudio { get; set; }
		public static void Reset()
		{
			BackgroundAudio?.Stop();
			MenuItemAudio?.Stop();

			BackgroundAudio = MenuItemAudio = null;

			while(Assets2Dispose.TryDequeue(out var asset))
				asset.Dispose();
		}
	}
}
