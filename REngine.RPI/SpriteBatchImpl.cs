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

		public IRenderFeature Feature { get => pFeature; }

		public SpriteBatchImpl(
			SpriteTextureManager texManager,
			SpriteBatcher batcher, 
			GraphicsSettings settings, 
			RenderSettings renderSettings,
			EngineEvents engineEvents,
			RendererEvents renderEvents
		)
		{
			pTextureManager = texManager;
			pBatcher = batcher;
			pFeature = new SpriteBatchFeature(
				batcher, 
				texManager,
				settings, 
				renderSettings,
				renderEvents
			);
			engineEvents.OnBeginUpdate += HandleBeginUpdate;
		}

		private void HandleBeginUpdate(object sender, UpdateEventArgs args)
		{
			pBatcher.Reset();
			pTextureManager.Update();
		}

		public ISpriteBatch Draw(SpriteBatchInfo batchInfo)
		{
			pBatcher.Next(ref batchInfo);
			return this;
		}

		public ISpriteBatch SetTexture(byte slot, ITexture texture)
		{
			pTextureManager.SetTexture(slot, texture);
			return this;
		}

		public ISpriteBatch SetTexture(byte slot, Image image)
		{
			pTextureManager.SetTexture(slot, image);
			return this;
		}
	}
}
