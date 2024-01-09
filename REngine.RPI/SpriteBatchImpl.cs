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
using REngine.Core.Reflection;
using REngine.RPI.Events;

namespace REngine.RPI
{
#if RENGINE_SPRITEBATCH
	internal class SpriteBatchImpl : ISpriteBatch, IDisposable
	{
		private readonly SpriteRenderSystem pRenderSystem;
		private readonly SpriteInstancedRenderSystem pInstancedRenderSystem;
		private readonly ITextRenderer pTextRenderer;
		private readonly IServiceProvider pServiceProvider;
		private readonly EngineEvents pEngineEvents;
		
		private bool pDisposed;

		public SpriteBatchImpl(
			SpriteRenderSystem renderSystem,
			SpriteInstancedRenderSystem instanceRenderSystem,
			IServiceProvider provider,
			EngineEvents engineEvents,
			ITextRenderer textRenderer)
		{
			pRenderSystem = renderSystem;
			pInstancedRenderSystem = instanceRenderSystem;
			pServiceProvider = provider;
			pTextRenderer = textRenderer;

			pEngineEvents = engineEvents;
			engineEvents.OnBeforeStop += OnEngineStop;
		}

		private void OnEngineStop(object? sender, EventArgs e)
		{
			pEngineEvents.OnBeforeStop -= OnEngineStop;
			Dispose();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			
			pRenderSystem.DestroyBatches();
			pInstancedRenderSystem.DestroyBatches();
			pDisposed = true;
		}

		public SpriteFeature CreateRenderFeature()
		{
			return ActivatorExtended.CreateInstance<SpriteFeature>(pServiceProvider) ?? throw new NullReferenceException();
		}
		public SpriteRenderItem CreateSprite(SpriteEffect? effect)
		{
			return pRenderSystem.Create(effect);
		}
		public InstancedSprite CreateSprite(SpriteInstancedCreateInfo createInfo)
		{
			return pInstancedRenderSystem.CreateBatch(createInfo);
		}

		public TextRendererBatch CreateText(in TextCreateInfo createInfo)
		{
			pTextRenderer.SetFont(createInfo.Font);
			var batch = pTextRenderer.CreateBatch(createInfo.Font.Name);
			batch.Lock();
			batch.Text = createInfo.Text;
			batch.Color = createInfo.Color;
			batch.Unlock();
			return batch;
		}
	}
#endif
}
