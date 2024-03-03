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
using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.RPI.Batches;
using REngine.RPI.Effects;
using REngine.RPI.Features;
using REngine.RPI.Resources;
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
		private SpriteBatch? pFlickeredDoge;
		private SpriteBatch? pColoredDoge;
		private TextureSpriteEffect? pSpriteEffect;
		public IWindow? Window { get; set; }
		public void Dispose()
		{
			renderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();
			pSpriteEffect?.Dispose();
			
			pFlickeredDoge?.Dispose();
			pColoredDoge?.Dispose();
		}

		public async Task Load(IServiceProvider provider)
		{
			// Load Sprite
			var spriteTex = await assetManager.GetAsyncAsset<TextureAsset>("Textures/doge.jpg");
			// Set Sprite on Spritebatch
			pSpriteFeature = ActivatorExtended.CreateInstance<SpriteFeature>(provider);
			renderer.AddFeature(pSpriteFeature);
			
			pSpriteEffect = TextureSpriteEffect.Build(provider);
			pSpriteEffect.Texture = spriteTex.Texture;
			
			pFlickeredDoge = spriteBatch.CreateSprite();
			pColoredDoge = spriteBatch.CreateSprite();
		}

		public void Update(IServiceProvider provider)
		{
			if (pColoredDoge is null || pFlickeredDoge is null || pSpriteEffect is null)
				return;
			var elapsedTime = (float)engine.ElapsedTime / 1000.0f;
			var wndSize = Window?.Size ?? new Size();
			var halfSize = new Vector3(wndSize.Width / 2.0f, wndSize.Height / 2.0f, 0);

			var stagger = QuadTime(elapsedTime + 0.5f, 2.5f, 3);
			var sineT = stagger * (float)Math.Sin(elapsedTime);
			var cosT = stagger * (float)Math.Cos(elapsedTime);
			
			pFlickeredDoge.Update(new SpriteBatchItemDesc()
			{
				Enabled = true,
				Effect = pSpriteEffect,
				Item = new SpriteBatchItem()
				{
					Scale = new Vector2(300) * QuadTime(elapsedTime, 1f, 2),
					Rotation = elapsedTime,
					Anchor = new Vector2(0.5f),
					Position = halfSize + (new Vector3(cosT, sineT, 0) * 150),
					Color = Color.White
				}
			});
			
			pColoredDoge.Update(new SpriteBatchItemDesc()
			{
				Enabled = true,
				Effect = pSpriteEffect,
				Item = new SpriteBatchItem()
				{
					Rotation = elapsedTime,
					Anchor = new Vector2(0.5f),
					Position = halfSize - new Vector3(75, 75, 0),
					Scale = new Vector2(150),
					Color = ColorUtils.FromHSL(elapsedTime, 1, 1)
				}
			});
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
