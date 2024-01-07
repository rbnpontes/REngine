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
		private Sprite? pFlickeredDoge;
		private Sprite? pColoredDoge;
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

		public void Load(IServiceProvider provider)
		{
			// Load Sprite
			var spriteTex = assetManager.GetAsset<TextureAsset>("Textures/doge.jpg");
			// Set Sprite on Spritebatch
			pSpriteFeature = ActivatorExtended.CreateInstance<SpriteFeature>(provider);
			renderer.AddFeature(pSpriteFeature);
			
			pSpriteEffect = TextureSpriteEffect.Build(provider);
			pSpriteEffect.Texture = spriteTex.Texture;
			
			pFlickeredDoge = spriteBatch.CreateSprite(pSpriteEffect);
			pColoredDoge = spriteBatch.CreateSprite(pSpriteEffect);
			
			pFlickeredDoge.Lock();
			pColoredDoge.Lock();
			pFlickeredDoge.Color = Color.White;
			pFlickeredDoge.Unlock();
			pColoredDoge.Unlock();
		}

		public void Update(IServiceProvider provider)
		{
			if (pColoredDoge is null || pFlickeredDoge is null)
				return;
			var elapsedTime = (float)engine.ElapsedTime / 1000.0f;
			var wndSize = Window?.Size ?? new Size();
			var halfSize = new Vector3(wndSize.Width / 2.0f, wndSize.Height / 2.0f, 0);

			var stagger = QuadTime(elapsedTime + 0.5f, 2.5f, 3);
			var sineT = stagger * (float)Math.Sin(elapsedTime);
			var cosT = stagger * (float)Math.Cos(elapsedTime);
			
			pFlickeredDoge.Lock();
			pFlickeredDoge.Size = new Vector2(300) * QuadTime(elapsedTime, 1f, 2);
			pFlickeredDoge.Angle = elapsedTime;
			pFlickeredDoge.Anchor = new Vector2(0.5f, 0.5f);
			pFlickeredDoge.Position = halfSize + (new Vector3(cosT, sineT, 0) * 150);
			pFlickeredDoge.Unlock();
			
			pColoredDoge.Lock();
			pColoredDoge.Angle = elapsedTime;
			pColoredDoge.Anchor = new Vector2(0.5f);
			pColoredDoge.Position = halfSize;
			pColoredDoge.Size = new Vector2(150);
			pColoredDoge.Color = ColorUtils.FromHSL(elapsedTime, 1, 1);
			pColoredDoge.Unlock();
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
