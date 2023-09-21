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
	internal class SpriteBatcher
	{
		private uint NextItemIdx = 0;
		private object pSync = new object();

		public SpriteBatchInfo[] Items { get; set; }
		public uint BatchCount { get => NextItemIdx; }

		public SpriteBatcher(RenderSettings settings)
		{
			Items = new SpriteBatchInfo[settings.SpriteBatchInitialSize];
		}

		public void Next(ref SpriteBatchInfo next)
		{
			uint nextItemIdx = 0;
			
			lock (pSync)
			{
				if (NextItemIdx >= Items.Length)
					RefitBatches();
				nextItemIdx = NextItemIdx;
				++NextItemIdx;	
			}

			Items[nextItemIdx] = next;
		}

		public void Reset()
		{
			NextItemIdx = 0;
		}

		private void RefitBatches()
		{
			var oldItems = Items;
			Items = new SpriteBatchInfo[Items.Length * 2];
			Array.Copy(Items, 0, oldItems, 0, oldItems.Length);
		}
	}
}
