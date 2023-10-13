using REngine.Core;
using REngine.Core.Resources;
using REngine.Core.Threading;
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
#if RENGINE_SPRITEBATCH
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
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly RPIEvents pRenderEvents;

		private SpriteBatchFeature? pFeature;

		public IRenderFeature Feature 
		{ 
			get
			{
				return GetFeature();
			}
		}
		public event EventHandler? OnDraw;

		public bool IsReady
		{
			get => pTextureManager.IsReady;
		}

		public SpriteBatchImpl(
			SpriteTextureManager texManager,
			SpriteBatcher batcher, 
			GraphicsSettings settings, 
			EngineEvents engineEvents,
			RPIEvents rpiEvents,
			RenderSettings renderSettings,
			IExecutionPipeline execPipeline
		)
		{
			pTextureManager = texManager;
			pBatcher = batcher;
			pRenderSettings = renderSettings;
			pGraphicsSettings = settings;
			pEngineEvents = engineEvents;
			pRenderEvents = rpiEvents;
			pExecutionPipeline = execPipeline;

			engineEvents.OnStart += HandleStart;
			engineEvents.OnStop += HandleStop;
			rpiEvents.OnUpdateSettings += HandleUpdateSettings;
			pTextureManager.OnUpdateTextures += HandleUpdateTextures;
			pTextureManager.OnRebuildTextures += HandleRebuildTextures;
		}

		private void HandleRebuildTextures(object? sender, EventArgs e)
		{
			GetFeature().UpdateBindings();
		}

		private void HandleUpdateTextures(object? sender, EventArgs e)
		{
			GetFeature().UpdateTextures();
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			pTextureManager.RecreateTextures();
			GetFeature().CheckCBufferSizes(e.Settings.ObjectBufferSize);
			pBatcher.UpdateSettings();
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pTextureManager.Start();
			pExecutionPipeline.AddEvent(DefaultEvents.SpriteBatchDrawId, (_) => HandleDraw());
		}

		private void HandleStop(object? sender, EventArgs e)
		{
			pBatcher.Reset();
			pTextureManager.Dispose();
			pFeature?.Dispose();
			pFeature = null;

			pEngineEvents.OnStart -= HandleStart;
			pEngineEvents.OnStop -= HandleStop;
			pRenderEvents.OnUpdateSettings -= HandleUpdateSettings;
			pTextureManager.OnUpdateTextures -= HandleUpdateTextures;
			pTextureManager.OnRebuildTextures -= HandleRebuildTextures;
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
			pFeature ??= new SpriteBatchFeature(pBatcher, pTextureManager, pGraphicsSettings);
			if (pFeature.IsDisposed)
				pFeature = new SpriteBatchFeature(pBatcher, pTextureManager, pGraphicsSettings);
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
				pBatcher.Add(textureSlot, item);
			return this;
		}
	}
#endif
}
