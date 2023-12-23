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
		private SpriteInstance? pBatch;

		private int pGridSize = 30;
		
		public IWindow? Window { get; set; }

		public void Dispose()
		{
			renderer?.RemoveFeature(pSpriteFeature);
			pSpriteFeature?.Dispose();

			pBatch?.Dispose();

			GC.SuppressFinalize(this);

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void Load(IServiceProvider provider)
		{
			// Load Sprite
			var sprite = assetManager.GetAsset<TextureAsset>("Textures/doge.jpg");
			// Allocate Batch
			pBatch = spriteBatch.CreateInstancedBatch((uint)(pGridSize * pGridSize), true);
			pBatch.Lock();
			pBatch.Texture = sprite.Texture;
			pBatch.Color = Color.White;
			pBatch.Unlock();
			
			// Allocate Render Feature
			pSpriteFeature = ActivatorExtended.CreateInstance<SpriteFeature>(provider);
			renderer.AddFeature(pSpriteFeature);
			
			imGuiSystem.OnGui += OnGui;
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("SpriteBatchInstanced Sample");

			ImGui.SliderInt("Num Instances", ref pGridSize, 1, 100);
			
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
		
		private void UpdateInstances(SpriteInstance batch, float elapsed, Size wndSize)
		{
			var size = new Vector2((5 + ((1 + (float)Math.Sin(elapsed)) * 0.5f) * 100));
			var anchor = new Vector2(0.5f, 0.5f);

			batch.Lock();
			batch.ResizeInstances((uint)(pGridSize * pGridSize), true);
			
			for (var i = 0u; i < batch.InstanceCount; ++i)
			{
				var x = i % pGridSize;
				var y = i / pGridSize;
				
				batch
					.SetInstanceScale(i, size)
					.SetInstanceAnchor(i, anchor)
					.SetInstancePosition(i, new Vector2(
						(x / ((float)pGridSize - 1)) * wndSize.Width,
						(y / ((float)pGridSize - 1)) * wndSize.Height
					))
					.SetInstanceAngle(i, (x - y) + elapsed);
			}
			batch.Unlock();
		}
	}
#endif
}
