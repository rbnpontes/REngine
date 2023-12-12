using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
#if RENGINE_SPRITEBATCH
	[Sample("SpriteBatch")]
	internal class SpriteBatchSample(
		ISpriteBatch spriteBatch,
		IRenderer renderer,
		IEngine engine,
		IAssetManager assetManager
		) : ISample
	{
		private IRenderFeature? pSpriteFeature;
		public IWindow? Window { get; set; }
		public void Dispose()
		{
			renderer?.RemoveFeature(pSpriteFeature);
			spriteBatch.ClearTextures();
			pSpriteFeature?.Dispose();

			spriteBatch.OnDraw -= OnDraw;
		}

		public void Load(IServiceProvider provider)
		{
			// Load Sprite
			ImageAsset sprite = assetManager.GetAsset<ImageAsset>("Textures/doge.jpg");
			// Set Sprite on Spritebatch
			spriteBatch.SetTexture(0, sprite.Image);
			pSpriteFeature = spriteBatch.Feature;

			renderer.AddFeature(pSpriteFeature);
			spriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if (!spriteBatch.IsReady)
				return;

			var elapsedTime = (float)engine.ElapsedTime / 1000.0f;
			var wndSize = Window?.Size ?? new Size();
			var halfSize = new Vector2(wndSize.Width / 2.0f, wndSize.Height / 2.0f);

			var stagger = QuadTime(elapsedTime + 0.5f, 2.5f, 3);
			var sineT = stagger * (float)Math.Sin(elapsedTime);
			var cosT = stagger * (float)Math.Cos(elapsedTime);

			// Draw Flickering Doge
			spriteBatch.Draw(new SpriteBatchInfo
			{
				/*Batch texture slot*/
				TextureSlot = 0,
				Size = new Vector2(300) * QuadTime(elapsedTime, 1f, 2),
				Angle = elapsedTime,
				Anchor = new Vector2(0.5f, 0.5f),
				Position = halfSize + (new Vector2(cosT, sineT) * 150)
			});
			// Draw Colored Doge
			spriteBatch.Draw(new SpriteBatchInfo
			{
				TextureSlot = 0,
				Angle = elapsedTime,
				Anchor = new Vector2(0.5f, 0.5f),
				Position = halfSize,
				Size = new Vector2(150),
				Color = ColorUtils.FromHSL(elapsedTime, 1, 1)
			});
		}

		public void Update(IServiceProvider provider)
		{
		}

		private static float QuadTime(float t, float freq, float amplitude)
		{
			t = (float)(Math.Sin(t * freq) * amplitude);
			t = Math.Clamp(t, -(float)Math.Round(freq), (float)Math.Round(freq));
			return (float)Math.Floor(t);
		}
	}
#endif
}
