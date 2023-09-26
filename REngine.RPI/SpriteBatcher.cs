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
	//internal struct SpriteBatchInstancedData
	//{
	//	public Vector4 PositionAndScale;
	//	public Vector4 RotationAndAnchor;

	//	public void Copy(ref SpriteInstancedBatchInfo batch)
	//	{
	//		PositionAndScale.X = batch.Position.X;
	//		PositionAndScale.Y = batch.Position.Y;
	//		PositionAndScale.Z = batch.Size.X;
	//		PositionAndScale.W = batch.Size.Y;

	//		RotationAndAnchor.X = batch.Anchor.X;
	//		RotationAndAnchor.Y = batch.Anchor.Y;
	//		RotationAndAnchor.Z = batch.Angle;
	//	}
	//}
	internal class SpriteBatcher
	{
		public BatchList<SpriteBatchInfo> Items { get; private set; }
		public BatchList<(byte, IEnumerable<SpriteInstancedBatchInfo>)> InstancedItems { get; set; }

		//public SpriteBatchInstancedData[] InstancedData { get; set; } = Array.Empty<SpriteBatchInstancedData>();
		//private int pMaxInstancedObjects = 0;

		public SpriteBatcher(RenderSettings settings, RPIEvents renderEvents)
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
			//pMaxInstancedObjects = Math.Max(pMaxInstancedObjects, instances.Count());
			InstancedItems.Add((textureSlot, instances));
		}

		//public void Update()
		//{
		//	if (InstancedData.Length > pMaxInstancedObjects)
		//		InstancedData = new SpriteBatchInstancedData[pMaxInstancedObjects];
		//}

		//public void CollectInstances(uint instanceId)
		//{
		//	if (instanceId >= InstancedItems.Count)
		//		return;

		//	var (_, instances) = InstancedItems[(int)instanceId];
		//	for(int i =0; i < instances.Count(); ++i)
		//	{
		//		var instance = instances.ElementAt(i);
		//		InstancedData[i].Copy(ref instance);
		//	}
		//}

		public void Reset()
		{
			Items.Clear();
			InstancedItems.Clear();
		}
	}
}
