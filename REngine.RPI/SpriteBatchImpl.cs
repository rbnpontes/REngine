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
using REngine.RPI.Events;

namespace REngine.RPI
{
#if RENGINE_SPRITEBATCH
	internal class SpriteBatchImpl(
		SpriteInstancedRenderSystem instanceRenderSystem,
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
		public SpriteBatchItem CreateSprite()
		{
			var batch = new SpriteBatchItem(bufferManager, this, provider.Get<IGraphicsDriver>().Backend);
			pBatchGroup.Lock();
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
			return batch;
		}
		public InstancedSprite CreateSprite(SpriteInstancedCreateInfo createInfo)
		{
			return instanceRenderSystem.CreateBatch(createInfo);
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

		public void RemoveBatch(SpriteBatchItem batchItem)
		{
			pBatchGroup.Lock();
			pBatchGroup.RemoveBatch(batchItem);
			pBatchGroup.Unlock();
		}
	}
#endif
}
