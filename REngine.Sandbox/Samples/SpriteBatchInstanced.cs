using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
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
	[Sample("SpriteBatch - Instanced")]
	internal class SpriteBatchInstanced : ISample
	{
		public IWindow? Window { get; set; }

		private ISpriteBatch? pSpriteBatch;
		private IRenderFeature? pSpriteFeature;
		private IRenderer? pRenderer;
		private IEngine? pEngine;

		private Size pInstancedSpriteSize;
		private SpriteInstancedBatchInfo[] pSpriteInstances;

		public SpriteBatchInstanced()
		{
			pInstancedSpriteSize = new Size(20, 20);
			pSpriteInstances = new SpriteInstancedBatchInfo[pInstancedSpriteSize.Width * pInstancedSpriteSize.Height];
		}

		public void Dispose()
		{
			pSpriteInstances = new SpriteInstancedBatchInfo[0];
			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteBatch?.ClearTextures();
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
			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0) / 1000.0f;
			Size wndSize = Window?.Size ?? new Size();

			pSpriteBatch?.Draw(0, GetSpriteInstances(elapsedTime, wndSize));
		}

		private IEnumerable<SpriteInstancedBatchInfo> GetSpriteInstances(float elapsed, Size wndSize)
		{
			Vector2 size = new Vector2((5 + ((1 + (float)Math.Sin(elapsed)) * 0.5f) * 100));
			Vector2 anchor = new Vector2(0.5f, 0.5f);
			return pSpriteInstances
					.AsParallel()
					.Select((sprite, i) =>
					{
						int x = i % pInstancedSpriteSize.Width;
						int y = i / pInstancedSpriteSize.Height;
						return new SpriteInstancedBatchInfo
						{
							Size = size,
							Angle = (x - y) + elapsed,
							Anchor = anchor,
							Position = new Vector2(
								(x / 20.0f) * wndSize.Width,
								(y / 20.0f) * wndSize.Height
							)
						};
					});
		}
	}
#endif
}
