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
		enum Step
		{
			Begin,
			Draw,
			End
		}

		private readonly SpriteBatcher pBatcher;
		private readonly SpriteTextureManager pTextureManager;
		private readonly RenderSettings pRenderSettings;
		private readonly GraphicsSettings pGraphicsSettings;

		private SpriteBatchFeature? pFeature;

		public IRenderFeature Feature 
		{ 
			get
			{
				var feature = pFeature;
				if(feature is null || feature?.IsDisposed == true)
					pFeature = feature = new SpriteBatchFeature(pBatcher, pTextureManager, pGraphicsSettings);
#pragma warning disable CS8603 // Possible null reference return.
				return feature;
#pragma warning restore CS8603 // Possible null reference return.
			}
		}

		public bool IsReady
		{
			get => pTextureManager.IsReady;
		}

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
			pRenderSettings = renderSettings;
			pGraphicsSettings = settings;

			engineEvents.OnStart += HandleStart;
			engineEvents.OnStop += HandleStop;
			rendererEvents.OnUpdateSettings += HandleUpdateSettings;
			pTextureManager.OnUpdateTextures += HandleUpdateTextures;
		}

		private void HandleUpdateTextures(object? sender, EventArgs e)
		{
			pFeature?.UpdateTextures();
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			pFeature?.CheckCBufferSizes(e.Settings.ObjectBufferSize);
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pTextureManager.Start();
		}

		private void HandleStop(object? sender, EventArgs e)
		{
			pBatcher.Reset();
			pTextureManager.Dispose();
			pFeature?.Dispose();
			pFeature = null;
		}

		private void HandleBeginUpdate(object? sender, UpdateEventArgs args)
		{
			if (pFeature?.IsDisposed == true)
				pFeature = null;
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
			pTextureManager.ClearTextures();
			return this;
		}

		public Task WaitTasks()
		{
			return Task.Run(() =>
			{
				pTextureManager.WaitTasks();
			});
		}
	}
}
