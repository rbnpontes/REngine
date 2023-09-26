using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox.Samples
{
#if RENGINE_SPRITEBATCH
	[Sample("SpriteBatch")]
	internal class SpriteBatchSample : ISample
	{
		public IWindow? Window { get; set; }
		private ISpriteBatch? pSpriteBatch;
		private IRenderFeature? pSpriteFeature;
		private IRenderer? pRenderer;
		private IEngine? pEngine;

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteBatch?.ClearTextures();
			pSpriteFeature?.Dispose();
		}

		public void Load(IServiceProvider provider)
		{
			pSpriteBatch = provider.Get<ISpriteBatch>();

			// Load Sprite
			ImageAsset sprite = new ImageAsset("doge.png");
			using (FileStream stream = new FileStream(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			// Set Sprite on Spritebatch
			pSpriteBatch.SetTexture(0, sprite.Image);
			pSpriteFeature = pSpriteBatch.Feature;

			pRenderer = provider.Get<IRenderer>().AddFeature(pSpriteFeature);
			pEngine = provider.Get<IEngine>();
		}

		public void Update(IServiceProvider provider)
		{
			if (pSpriteBatch?.IsReady == false)
				return;

			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0) / 1000.0f;
			Size wndSize = Window?.Size ?? new Size();
			Vector2 halfSize = new Vector2(wndSize.Width / 2.0f, wndSize.Height / 2.0f);

			float stagger = AnalogicTime(elapsedTime + 0.5f, 2.5f, 3);
			float sineT = stagger * (float)Math.Sin(elapsedTime);
			float cosT = stagger * (float)Math.Cos(elapsedTime);

			// Draw Flickering Doge
			pSpriteBatch?.Draw(new SpriteBatchInfo
			{
				/*Batch texture slot*/
				TextureSlot = 0,
				Size = new Vector2(300) * AnalogicTime(elapsedTime, 1f, 2),
				Angle = elapsedTime,
				Anchor = new Vector2(0.5f, 0.5f),
				Position = halfSize + (new Vector2(cosT, sineT) * 150)
			});
			// Draw Colored Doge
			pSpriteBatch?.Draw(new SpriteBatchInfo
			{
				TextureSlot = 0,
				Angle = elapsedTime,
				Anchor = new Vector2(0.5f, 0.5f),
				Position = halfSize,
				Size = new Vector2(150),
				Color = ColorUtils.FromHSL(elapsedTime, 1, 1)
			});
		}

		private float AnalogicTime(float t, float freq, float amplitude)
		{
			t = (float)(Math.Sin(t * freq) * amplitude);
			t = Math.Clamp(t, -(float)Math.Round(freq), (float)Math.Round(freq));
			return (float)Math.Floor(t);
		}
	}
#endif
}
