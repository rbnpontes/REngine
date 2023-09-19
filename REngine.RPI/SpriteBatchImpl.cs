using REngine.Core.Resources;
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
		public IRenderFeature Feature { get; private set; }

		public SpriteBatchImpl(SpriteBatcher batcher)
		{
			pBatcher = batcher;
			Feature = new SpriteBatchFeature(batcher);
		}

		public ISpriteBatch Begin()
		{
			pBatcher.Reset();
			return this;
		}

		public ISpriteBatch Draw(SpriteBatchInfo batchInfo)
		{
			var item = pBatcher.Next();
			item.Position = batchInfo.Position;
			item.Size = batchInfo.Size;
			item.Image = batchInfo.Image;
			return this;
		}

		public ISpriteBatch End()
		{
			return this;
		}
	}
}
