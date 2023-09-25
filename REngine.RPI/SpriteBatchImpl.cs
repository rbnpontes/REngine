using REngine.Core;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class SpriteBatchImpl : ISpriteBatch
	{
		private SpriteBatcher pBatcher;
		private SpriteBatchFeature pFeature;
		private SpriteTextureManager pTextureManager;
		private RenderSettings pRenderSettings;

		public IRenderFeature Feature { get => pFeature; }

		public SpriteBatchImpl(
			SpriteTextureManager texManager,
			SpriteBatcher batcher, 
			GraphicsSettings settings, 
			EngineEvents engineEvents,
			RPIEvents rendererEvents,
			RenderSettings renderSettings
		)
		{
			pTextureManager = texManager;
			pBatcher = batcher;
			pFeature = new SpriteBatchFeature(
				batcher, 
				texManager,
				settings
			);
			pRenderSettings = renderSettings;

			engineEvents.OnStart += HandleStart;
			engineEvents.OnBeginUpdate += HandleBeginUpdate;
			engineEvents.OnStop += HandleStop;
			rendererEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			pFeature.CheckCBufferSizes(e.Settings.ObjectBufferSize);
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pTextureManager.Start();
		}

		private void HandleStop(object? sender, EventArgs e)
		{
			pBatcher.Reset();
			pTextureManager.Dispose();
			pFeature.Dispose();
		}

		private void HandleBeginUpdate(object sender, UpdateEventArgs args)
		{
			pBatcher.Reset();
			pTextureManager.Update();
		}

		public ISpriteBatch Draw(SpriteBatchInfo batchInfo)
		{
			pBatcher.Add(batchInfo);
			return this;
		}

		public ISpriteBatch Draw(byte textureSlot, IEnumerable<SpriteInstancedBatchInfo> instances)
		{
			AssertSlot(textureSlot);
			pBatcher.Add(textureSlot, instances);
			return this;
		}

		public ISpriteBatch SetTexture(byte slot, ITexture texture)
		{
			AssertSlot(slot);
			pTextureManager.SetTexture(slot, texture);
			return this;
		}

		public ISpriteBatch SetTexture(byte slot, Image image)
		{
			AssertSlot(slot);
			pTextureManager.SetTexture(slot, image);
			return this;
		}

		private void AssertSlot(byte slot)
		{
			if (slot >= pRenderSettings.SpriteBatchMaxTextures)
				throw new ArgumentOutOfRangeException($"Slot is greater than max allowed sprite textures. Increase value on Render Settings. Max={pRenderSettings.SpriteBatchMaxTextures}");
		}

		public ISpriteBatch ClearTexture(byte slot)
		{
			AssertSlot(slot);
			pTextureManager.SetTextureNull(slot);
			return this;
		}

		public ISpriteBatch ClearTextures()
		{
			for(byte slot = 0; slot < pRenderSettings.SpriteBatchMaxTextures; ++slot)
			{
				ClearTexture(slot);
			}
			return this;
		}

	}
}
