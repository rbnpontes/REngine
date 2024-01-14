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
using ImGuiNET;
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
	[Sample("SpriteBatch - Instanced")]
	internal class SpriteBatchInstanced(
		ISpriteBatch spriteBatch,
		IRenderer renderer,
		IEngine engine,
		IAssetManager assetManager,
		IImGuiSystem imGuiSystem) : ISample
	{
		private IRenderFeature? pSpriteFeature;
		
		private DynamicSpriteInstanceBatch? pBatch;
		private TextureSpriteEffect? pEffect;
		private SpriteBatchItem[] pItems = [];
		
		private int pGridSize = 30;
		public IWindow? Window { get; set; }

		public void Dispose()
		{
			renderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();
			pEffect?.Dispose();
			pBatch?.Dispose();
			
			GC.SuppressFinalize(this);

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void Load(IServiceProvider provider)
		{
			// Load Sprite
			var sprite = assetManager.GetAsset<TextureAsset>("Textures/doge.jpg");
			pEffect = TextureSpriteEffect.Build(provider);
			pEffect.Texture = sprite.Texture;
			// Allocate Batch
			pBatch = spriteBatch.CreateDynamicSprite();
			pItems = new SpriteBatchItem[pGridSize * pGridSize];
			
			// Allocate Render Feature
			pSpriteFeature = ActivatorExtended.CreateInstance<SpriteFeature>(provider);
			renderer.AddFeature(pSpriteFeature);
			
			imGuiSystem.OnGui += OnGui;
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("SpriteBatchInstanced Sample");

			ImGui.SliderInt("Num Instances", ref pGridSize, 1, 100);
			if ((pGridSize * pGridSize) != pItems.Length)
				pItems = new SpriteBatchItem[pGridSize * pGridSize];
			
			ImGui.End();
		}

		public void Update(IServiceProvider provider)
		{
			if (pBatch is null)
				return;
			
			var elapsedTime = (float)engine.ElapsedTime / 1000.0f;
			var wndSize = Window?.Size ?? new Size();

			UpdateInstances(pBatch, elapsedTime, wndSize);
		}
		
		private void UpdateInstances(DynamicSpriteInstanceBatch batch, float elapsed, Size wndSize)
		{
			var size = new Vector2((5 + ((1 + (float)Math.Sin(elapsed)) * 0.5f) * 100));
			var anchor = new Vector2(0.5f, 0.5f);
			
			for (var i = 0u; i < pItems.Length; ++i)
			{
				var x = i % pGridSize;
				var y = i / pGridSize;

				pItems[i] = new SpriteBatchItem()
				{
					Color = Color.White,
					Anchor = anchor,
					Position = new Vector3(
						(x / ((float)pGridSize - 1)) * wndSize.Width,
						(y / ((float)pGridSize - 1)) * wndSize.Height,
						0
					),
					Rotation = (x - y) + elapsed,
					Scale = size
				};
			}
			
			batch.Update(new SpriteInstanceBatchItemDesc()
			{
				Enabled = true,
				Effect = pEffect,
				Items = pItems
			});
		}
	}
#endif
}
