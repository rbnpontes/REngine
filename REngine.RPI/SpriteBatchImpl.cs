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
using REngine.Core.DependencyInjection;
using REngine.Core.Reflection;
using REngine.RPI.Batches;
using REngine.RPI.Effects;
using REngine.RPI.Events;
using REngine.RPI.Structs;

namespace REngine.RPI
{
#if RENGINE_SPRITEBATCH
	internal class SpriteBatchImpl(
		IServiceProvider provider,
		ITextRenderer textRenderer,
		BatchSystem batchSystem,
		IBufferManager bufferManager)
		: ISpriteBatch
	{
		private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(BatchGroupNames.Sprites);

		public SpriteEffect DefaultEffect => SpriteEffect.Build(provider);

		public SpriteFeature CreateRenderFeature()
		{
			return ActivatorExtended.CreateInstance<SpriteFeature>(provider) ?? throw new NullReferenceException();
		}
		public SpriteBatch CreateSprite()
		{
			var batch = new SpriteBatch(bufferManager, this);
			pBatchGroup.Lock();
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
			return batch;
		}

		public DynamicSpriteInstanceBatch CreateDynamicSprite()
		{
			var batch = new DynamicSpriteInstanceBatch(bufferManager, this);
			pBatchGroup.Lock();
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
			return batch;
		}

		public DefaultSpriteInstanceBatch CreateDefaultSprite()
		{
			var batch = new DefaultSpriteInstanceBatch(bufferManager, this);
			pBatchGroup.Lock();
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
			return batch;
		}

		public StaticSpriteInstanceBatch CreateStaticSprite()
		{
			var batch = new StaticSpriteInstanceBatch(bufferManager, this);
			pBatchGroup.Lock();
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
			return batch;
		}

		public TextRendererBatch CreateText(in TextCreateInfo createInfo)
		{
			textRenderer.SetFont(createInfo.Font);
			var batch = textRenderer.CreateBatch(createInfo.Font.Name);
			batch.Lock();
			batch.Text = createInfo.Text;
			batch.Color = createInfo.Color;
			batch.Unlock();
			return batch;
		}

		public void RemoveBatch(SpriteBatch batch)
		{
			pBatchGroup.Lock();
			pBatchGroup.RemoveBatch(batch);
			pBatchGroup.Unlock();
		}

		public void RemoveBatch(SpriteInstanceBatch batch)
		{
			pBatchGroup.Lock();
			pBatchGroup.RemoveBatch(batch);
			pBatchGroup.Unlock();
		}
	}
#endif
}
