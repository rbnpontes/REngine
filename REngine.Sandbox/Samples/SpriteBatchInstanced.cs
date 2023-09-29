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

		private ISpriteInstancing? pInstancingObject;

		private Size pInstancingSpriteGridSize;
		private SpriteInstancedBatchInfo[] pSpriteInstances;

		public SpriteBatchInstanced()
		{
			pInstancingSpriteGridSize = new Size(20, 20);
			pSpriteInstances = new SpriteInstancedBatchInfo[pInstancingSpriteGridSize.Width * pInstancingSpriteGridSize.Height];
		}

		public void Dispose()
		{
			pSpriteInstances = new SpriteInstancedBatchInfo[0];
			pRenderer?.RemoveFeature(pSpriteFeature);
			pSpriteBatch?.ClearTextures();
			pSpriteFeature?.Dispose();

			if(pSpriteBatch != null)
				pSpriteBatch.OnDraw -= OnDraw;

			GC.SuppressFinalize(this);
		}

		public void Load(IServiceProvider provider)
		{
			pSpriteBatch = provider.Get<ISpriteBatch>();

			// Load Sprite
			ImageAsset sprite = new("doge.png");
			using (FileStream stream = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg"), FileMode.Open))
				sprite.Load(stream).Wait();

			// Set Sprite on Spritebatch
			pSpriteBatch.SetTexture(0, sprite.Image);
			// Allocates Instancing Object
			pInstancingObject = pSpriteBatch.GetInstancing(pInstancingSpriteGridSize.Width * pInstancingSpriteGridSize.Height);

			pSpriteFeature = pSpriteBatch.Feature;

			pRenderer = provider.Get<IRenderer>().AddFeature(pSpriteFeature);
			pEngine = provider.Get<IEngine>();

			pSpriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if (pSpriteBatch?.IsReady == false || pInstancingObject is null)
				return;

			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0) / 1000.0f;
			Size wndSize = Window?.Size ?? new Size();

			UpdateInstances(pInstancingObject, elapsedTime, wndSize);
			pSpriteBatch?.Draw(0, pInstancingObject);
		}

		public void Update(IServiceProvider provider)
		{
		}
		
		private void UpdateInstances(ISpriteInstancing instancing, float elapsed, Size wndSize)
		{
			Vector2 size = new Vector2((5 + ((1 + (float)Math.Sin(elapsed)) * 0.5f) * 100));
			Vector2 anchor = new Vector2(0.5f, 0.5f);

			for (int i = 0; i < instancing.Length; ++i)
			{
				int x = i % pInstancingSpriteGridSize.Width;
				int y = i / pInstancingSpriteGridSize.Height;

				instancing
					.SetSize(i, size)
					.SetAnchor(i, anchor)
					.SetPosition(i, new Vector2(
						(x / 20.0f) * wndSize.Width,
						(y / 20.0f) * wndSize.Height
					))
					.SetAngle(i, (x - y) + elapsed);
			}
		}
	}
#endif
}
