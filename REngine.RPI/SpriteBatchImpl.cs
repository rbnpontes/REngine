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
		private readonly SpriteSystem pSystem;
		private readonly SpriteInstancedBatchSystem pInstancedBatchSystem;
		private readonly IServiceProvider pServiceProvider;
		private readonly EngineEvents pEngineEvents;
		
		private bool pDisposed;

		public SpriteBatchImpl(
			SpriteSystem system,
			SpriteInstancedBatchSystem instanceBatchSystem,
			IServiceProvider provider,
			EngineEvents engineEvents)
		{
			pSystem = system;
			pInstancedBatchSystem = instanceBatchSystem;
			pServiceProvider = provider;

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
			
			pSystem.DestroyBatches();
			pInstancedBatchSystem.DestroyBatches();
			pDisposed = true;
		}

		public SpriteFeature CreateRenderFeature()
		{
			return ActivatorExtended.CreateInstance<SpriteFeature>(pServiceProvider) ?? throw new NullReferenceException();
		}
		public Sprite CreateSprite(SpriteEffect? effect)
		{
			return pSystem.Create(effect);
		}
		public SpriteInstance CreateInstancedBatch(uint numInstances, bool dynamic = false)
		{
			return pInstancedBatchSystem.CreateBatch(numInstances, dynamic);
		}
	}
#endif
}
