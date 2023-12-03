using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.Sandbox.Samples
{
#if RENGINE_SPRITEBATCH
	[Sample("SpriteBatch - Instanced")]
	internal class SpriteBatchInstanced(
		ISpriteBatch spriteBatch,
		IRenderer renderer,
		IEngine engine,
		IAssetManager assetManager) : ISample
	{
		private readonly Size pInstancingSpriteGridSize = new Size(20, 20);

		private IRenderFeature? pSpriteFeature;
		private ISpriteInstancing? pInstancingObject;
		
		public IWindow? Window { get; set; }

		public void Dispose()
		{
			renderer?.RemoveFeature(pSpriteFeature);
			spriteBatch.ClearTextures();
			pSpriteFeature?.Dispose();

			spriteBatch.OnDraw -= OnDraw;

			pInstancingObject = null;

			GC.SuppressFinalize(this);

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void Load(IServiceProvider provider)
		{
			spriteBatch = provider.Get<ISpriteBatch>();

			// Load Sprite
			var sprite = assetManager.GetAsset<ImageAsset>("Textures/doge.jpg");

			// Set Sprite on Spritebatch
			spriteBatch.SetTexture(0, sprite.Image);
			// Allocates Instancing Object
			pInstancingObject = spriteBatch.GetInstancing(pInstancingSpriteGridSize.Width * pInstancingSpriteGridSize.Height);
			pSpriteFeature = spriteBatch.Feature;
			renderer.AddFeature(pSpriteFeature);

			spriteBatch.OnDraw += OnDraw;
		}

		private void OnDraw(object? sender, EventArgs e)
		{
			if (spriteBatch.IsReady == false || pInstancingObject is null)
				return;

			var elapsedTime = (float)engine.ElapsedTime / 1000.0f;
			var wndSize = Window?.Size ?? new Size();

			UpdateInstances(pInstancingObject, elapsedTime, wndSize);
			spriteBatch?.Draw(0, pInstancingObject);
		}

		public void Update(IServiceProvider provider)
		{
		}
		
		private void UpdateInstances(ISpriteInstancing instancing, float elapsed, Size wndSize)
		{
			var size = new Vector2((5 + ((1 + (float)Math.Sin(elapsed)) * 0.5f) * 100));
			var anchor = new Vector2(0.5f, 0.5f);

			for (var i = 0; i < instancing.Length; ++i)
			{
				var x = i % pInstancingSpriteGridSize.Width;
				var y = i / pInstancingSpriteGridSize.Height;

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
