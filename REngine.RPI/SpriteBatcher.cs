using REngine.Core.Collections;
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
		public BatchList<SpriteBatchInfo> Items { get; private set; }
		public BatchList<(byte, IEnumerable<SpriteInstancedBatchInfo>)> InstancedItems { get; set; }

		public SpriteBatcher(RenderSettings settings, RenderEvents renderEvents)
		{
			Items = new BatchList<SpriteBatchInfo>(settings.SpriteBatchInitialSize, settings.SpriteBatchGrowth);
			InstancedItems = new BatchList<(byte, IEnumerable<SpriteInstancedBatchInfo>)>(settings.SpriteBatchInitialSize, settings.SpriteBatchGrowth);
			renderEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			Items.GrowthSize = e.Settings.SpriteBatchGrowth;
			InstancedItems.GrowthSize = e.Settings.SpriteBatchGrowth;
		}

		public void Add(in SpriteBatchInfo next)
		{
			Items.Add(next);
		}
		public void Add(byte textureSlot, IEnumerable<SpriteInstancedBatchInfo> instances)
		{
			InstancedItems.Add((textureSlot, instances));
		}

		public void Reset()
		{
			Items.Clear();
			InstancedItems.Clear();
		}
	}
}
