using REngine.Core;
using REngine.Core.Resources;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Events;

namespace REngine.RPI
{
#if RENGINE_SPRITEBATCH
	internal class SpriteBatchImpl : ISpriteBatch, IDisposable
	{
		private readonly SpriteBatcher pBatcher;
		private readonly SpriteTextureManager pTextureManager;
		private readonly RenderSettings pRenderSettings;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly RPIEvents pRpiEvents;
		private readonly RendererEvents pRendererEvents;
		private readonly IServiceProvider pServiceProvider;

		private SpriteBatchFeature? pFeature;
		private bool pDisposed;

		public IGraphicsRenderFeature Feature => GetFeature();
		public event EventHandler? OnDraw;

		public bool IsReady => pTextureManager.IsReady;

		public SpriteBatchImpl(
			SpriteTextureManager texManager,
			SpriteBatcher batcher, 
			GraphicsSettings settings,
			RPIEvents rpiEventsEvents,
			RendererEvents rendererEvents,
			RenderSettings renderSettings,
			IExecutionPipeline execPipeline,
			IServiceProvider provider
		)
		{
			pTextureManager = texManager;
			pBatcher = batcher;
			pRenderSettings = renderSettings;
			pGraphicsSettings = settings;
			pRpiEvents = rpiEventsEvents;
			pRendererEvents = rendererEvents;
			pExecutionPipeline = execPipeline;
			pServiceProvider = provider;

			rendererEvents.OnReady += HandleRendererReady;
			rendererEvents.OnDispose += HandleRendererDispose;
			rpiEventsEvents.OnUpdateSettings += HandleUpdateSettings;
			pTextureManager.OnUpdateTextures += HandleUpdateTextures;
			pTextureManager.OnRebuildTextures += HandleRebuildTextures;
		}

		private void HandleRendererReady(object? sender, EventArgs e)
		{
			pRendererEvents.OnReady -= HandleRendererReady;

			pTextureManager.Start();
			pExecutionPipeline.AddEvent(DefaultEvents.SpriteBatchDrawId, (_) => HandleDraw());
		}

		private void HandleRebuildTextures(object? sender, EventArgs e)
		{
			GetFeature().UpdateBindings();
		}

		private void HandleUpdateTextures(object? sender, EventArgs e)
		{
			GetFeature().UpdateTextures();
		}

		private void HandleUpdateSettings(object? sender, EventArgs e)
		{
			if (sender is RenderSettings settings)
			{
				pTextureManager.RecreateTextures();
				GetFeature().CheckCBufferSizes(settings.ObjectBufferSize);
				pBatcher.UpdateSettings();
			}
		}

		private void HandleRendererDispose(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			
			pBatcher.Reset();
			pTextureManager.Dispose();
			pFeature?.Dispose();
			pFeature = null;

			pRpiEvents.OnUpdateSettings -= HandleUpdateSettings;
			pTextureManager.OnUpdateTextures -= HandleUpdateTextures;
			pTextureManager.OnRebuildTextures -= HandleRebuildTextures;

			pDisposed = true;
		}

		private void HandleDraw()
		{
			GetFeature();
			lock(pBatcher.SyncPrimitive)
				pBatcher.Reset();
			OnDraw?.Invoke(this, EventArgs.Empty);
		}

		private SpriteBatchFeature GetFeature()
		{
			pFeature ??= new SpriteBatchFeature(pBatcher, pTextureManager, pGraphicsSettings, pServiceProvider);
			if (pFeature.IsDisposed)
				pFeature = new SpriteBatchFeature(pBatcher, pTextureManager, pGraphicsSettings, pServiceProvider);
			return pFeature;
		}

		public ISpriteBatch Draw(SpriteBatchInfo batchInfo)
		{
			pBatcher.Add(batchInfo);
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

		public ISpriteInstancing GetInstancing(int length)
		{
			return pBatcher.Allocate(length);
		}

		public ISpriteBatch Draw(byte textureSlot, ISpriteInstancing instancingItem)
		{
			if(instancingItem is SpriteInstancing item)
				pBatcher.Add(textureSlot, null, item);
			return this;
		}

		public ISpriteBatch Draw(byte textureSlot, Color color, ISpriteInstancing instancingItem)
		{
			if(instancingItem is SpriteInstancing item)
				pBatcher.Add(textureSlot, color, item);
			return this;
		}

		public ISpriteBatch Draw(TextRendererBatch textBatch)
		{
			pBatcher.Add(textBatch);
			return this;
		}
	}
#endif
}
