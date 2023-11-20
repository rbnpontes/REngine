using REngine.Core.Collections;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal struct SpriteInstanceItem
	{
		public Vector4 PositionAndScale;
		public Vector4 RotationAndAnchor;
	}

	class SpriteInstancing : ISpriteInstancing
	{
		private readonly SpriteBatcher pBatcher;

		public int Offset { get; private set; }
		public int Length { get; private set; }


		public SpriteInstancing(SpriteBatcher batcher, int offset, int length)
		{
			pBatcher = batcher;
			Offset = offset;
			Length = length;
		}
		
		private void ValidateIndex(int idx)
		{
			if (idx < 0 || idx >= Length)
				throw new IndexOutOfRangeException(nameof(GetAnchor));
		}

		public Vector2 GetAnchor(int idx)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			return new Vector2(pBatcher.InstanceData[idx].RotationAndAnchor.X, pBatcher.InstanceData[idx].RotationAndAnchor.Y);
		}

		public float GetAngle(int idx)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			return pBatcher.InstanceData[idx].RotationAndAnchor.Z;
		}

		public Vector2 GetPosition(int idx)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			return new Vector2(pBatcher.InstanceData[idx].PositionAndScale.X, pBatcher.InstanceData[idx].PositionAndScale.Y);
		}

		public Vector2 GetSize(int idx)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			return new Vector2(pBatcher.InstanceData[idx].PositionAndScale.Z, pBatcher.InstanceData[idx].PositionAndScale.W);
		}

		public ISpriteInstancing SetAnchor(int idx, Vector2 anchor)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			var data = pBatcher.InstanceData[idx];
			data.RotationAndAnchor.X = anchor.X;
			data.RotationAndAnchor.Y = anchor.Y;
			pBatcher.InstanceData[idx] = data;
			return this;
		}

		public ISpriteInstancing SetAngle(int idx, float angle)
		{
			ValidateIndex(idx);
			idx = Offset + idx;
			var data = pBatcher.InstanceData[idx];
			data.RotationAndAnchor.Z = angle;
			pBatcher.InstanceData[idx] = data;
			return this;
		}

		public ISpriteInstancing SetPosition(int idx, Vector2 position)
		{
			ValidateIndex(idx);
			idx = Offset + idx;

			var data = pBatcher.InstanceData[idx];
			data.PositionAndScale.X = position.X;
			data.PositionAndScale.Y = position.Y;

			pBatcher.InstanceData[idx] = data;
			return this;
		}

		public ISpriteInstancing SetSize(int idx, Vector2 size)
		{
			ValidateIndex(idx);
			idx = Offset + idx;

			var data = pBatcher.InstanceData[idx];
			data.PositionAndScale.Z = size.X;
			data.PositionAndScale.W = size.Y;

			pBatcher.InstanceData[idx] = data;
			return this;
		}
	}

	internal class SpriteInstancingEntry
	{
		public WeakReference<SpriteInstancing> Instancing { get; set; }
		public byte TextureSlot { get; set; }
		public Color Color { get; set; } = Color.White;
		public SpriteInstancingEntry(byte slot, SpriteInstancing instancing)
		{
			TextureSlot = slot;
			Instancing = new WeakReference<SpriteInstancing>(instancing);
		}
	}

	internal class SpriteBatcher
	{
		private readonly RenderSettings pSettings;

		public BatchList<SpriteBatchInfo> Items { get; private set; }
		public BatchList<TextRendererBatch> TextBatches { get; private set; }
		public SpriteInstanceItem[] InstanceData { get; private set; }
		public int TotalInstanceItems { get; private set; }
		public ulong MaxAllocatedInstanceItems { get; private set; }

		public Dictionary<int, SpriteInstancingEntry> InstanceEntries { get; private set; } = new ();

		public object SyncPrimitive { get; private set; } = new();

		public SpriteBatcher(RenderSettings settings)
		{
			pSettings = settings;
			Items = new BatchList<SpriteBatchInfo>(
				settings.SpriteBatchInitialSize, 
				settings.SpriteBatchInitialSize * 2
			);
			TextBatches = new BatchList<TextRendererBatch>(
				settings.SpriteBatchInitialSize,
				settings.SpriteBatchTextsInitialSize * 2
			);
			InstanceData = new SpriteInstanceItem[settings.SpriteBatchInitialInstanceSize];
			TotalInstanceItems = 0;
		}

		public void UpdateSettings()
		{
			lock (SyncPrimitive)
			{
				Items.GrowthSize = pSettings.SpriteBatchInitialSize * 2;
				TextBatches.GrowthSize = pSettings.SpriteBatchTextsInitialSize * 2;
			}
		}

		private void ResizeInstanceData(int newLength)
		{
			newLength += InstanceData.Length;
			newLength += (int)Math.Floor(pSettings.SpriteBatchInitialInstanceSize * pSettings.SpriteBatchInstanceExpansionRatio);
			var oldData = InstanceData;
			InstanceData = new SpriteInstanceItem[newLength];

			Array.Copy(oldData, InstanceData, oldData.Length);
		}

		public SpriteInstancing Allocate(int length)
		{
			SpriteInstancing result;
			lock (SyncPrimitive)
			{
				if (length < 1)
					throw new Exception("Length cannot be less than 1");

				if (TotalInstanceItems + length > InstanceData.Length)
					ResizeInstanceData(length);
				result = new SpriteInstancing(this, TotalInstanceItems, length);
				TotalInstanceItems += length;
				MaxAllocatedInstanceItems = Math.Max(MaxAllocatedInstanceItems, (ulong)length);
			}
			return result;
		}
		
		public void Add(in SpriteBatchInfo next)
		{
			lock(SyncPrimitive)
				Items.Add(next);
		}
		public void Add(byte textureSlot, Color? color, SpriteInstancing instancing)
		{
			lock (SyncPrimitive)
			{
				int key = instancing.GetHashCode();

				if(InstanceEntries.TryGetValue(key, out var entry))
				{
					if (entry.TextureSlot != textureSlot)
						entry.TextureSlot = textureSlot;
					if(entry.Color != color && color != null)
						entry.Color = color.Value;
					return;
				}

				InstanceEntries.Add(key, new SpriteInstancingEntry(textureSlot, instancing));
			}
		}
		public void Add(TextRendererBatch batch)
		{
			lock(SyncPrimitive)
				TextBatches.Add(batch);
		}
		
		public void Reset()
		{
			lock (SyncPrimitive)
			{
				Items.Clear();
				TextBatches.Clear();
			}
		}
	}
}
