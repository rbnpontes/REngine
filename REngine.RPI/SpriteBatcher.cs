using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class SpriteBatchItem
	{
		public byte TextureIndex { get; set; } = byte.MaxValue;
		public Vector2 Position { get; set; } = Vector2.Zero;
		public Vector2 Offset { get; set; } = Vector2.Zero;
		public float Angle { get; set; } = 0f;
		public Vector2 Size { get; set; } = Vector2.One;
	}

	internal class SpriteBatcher
	{
		private uint NextItemIdx = 0;
		private object pSync = new object();

		public SpriteBatchItem[] Items { get; set; }
		public uint BatchCount { get => NextItemIdx; }

		public SpriteBatcher(RenderSettings settings)
		{
			Items = new SpriteBatchItem[settings.SpriteBatchInitialSize];
			for(int i =0; i < settings.SpriteBatchInitialSize; ++i)
				Items[i] = new SpriteBatchItem();
		}

		public SpriteBatchItem Next()
		{
			uint nextItemIdx = 0;
			
			lock (pSync)
			{
				if (NextItemIdx >= Items.Length)
					RefitBatches();
				nextItemIdx = NextItemIdx;
				++NextItemIdx;	
			}

			return Items[nextItemIdx];
		}

		public void Reset()
		{
			NextItemIdx = 0;
		}

		private void RefitBatches()
		{
			var oldItems = Items;
			Items = new SpriteBatchItem[Items.Length * 2];
			Array.Copy(Items, 0, oldItems, 0, oldItems.Length);
			for(int i = 0; i < Items.Length; ++i)
			{
				if (Items[i] is null)
					Items[i] = new SpriteBatchItem();
			}
		}
	}
}
