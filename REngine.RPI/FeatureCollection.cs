using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class FeatureCollection : IEnumerable<IRenderFeature>, IDisposable
	{
		class FeatureEntry : IComparable<FeatureEntry> 
		{
			public bool Removed { get; set; } = false;
			public IRenderFeature Feature { get; set; }
			public int ZIndex { get; set; } = -1;
			public int Index { get; set; } = -1;
			public LinkedListNode<FeatureEntry>? InsertNode { get; set; }

			public FeatureEntry(IRenderFeature feature)
			{
				Feature = feature;
			}

			public int CompareTo(FeatureEntry? other)
			{
				if (other is null)
					return -1;
				return (ZIndex + Index) - (other.Index + other.ZIndex);
			}
		}

		private readonly LinkedList<FeatureEntry> pFeatures2Insert = new();
		private readonly Dictionary<IRenderFeature, FeatureEntry> pLookupFeatures = new();

		private int pFeatures2RemoveCount = 0;

		private FeatureEntry[] pFeatureEntries = new FeatureEntry[0];

		private object pSync = new();

		public bool NeedsPrepare
		{
			get
			{
				bool result = false;
				lock(pSync)
					result = pFeatures2RemoveCount > 0 || pFeatures2Insert.Count > 0;
				return result;
			}
		}

		public void AddFeature(IRenderFeature feature, int zindex)
		{
			lock(pSync)
			{
#if DEBUG
				if (pLookupFeatures.ContainsKey(feature))
					throw new ArgumentException("Feature has been already added. You must remove feature first");
#endif
				FeatureEntry entry = new(feature) { ZIndex = zindex };

				var node = pFeatures2Insert.AddLast(entry);
				entry.InsertNode = node;
				pLookupFeatures[feature] = entry;
			}
		}

		public void AddFeatures(IEnumerable<IRenderFeature> features, int zindex)
		{
			foreach(var feat in features)
				AddFeature(feat, zindex);
		}

		public void RemoveFeature(IRenderFeature feature)
		{
			lock (pSync)
			{
				pLookupFeatures.TryGetValue(feature, out var entry);
				if (entry != null)
					RemoveEntry(entry);
			}
		}

		private void RemoveEntry(FeatureEntry entry)
		{
			entry.Removed = true;
			++pFeatures2RemoveCount;
			pLookupFeatures.Remove(entry.Feature);

			// If feature needs to be inserted, we must remove then
			if (entry.InsertNode != null)
			{
				pFeatures2Insert.Remove(entry.InsertNode);
				entry.InsertNode = null;
			}
		}

		public void Prepare()
		{
			lock (pSync)
				RemoveFeatures();

			bool needsSort = false;
			lock (pSync)
				needsSort = InsertFeatures();

			lock(pSync)
			{
				if (needsSort)
					SortFeatures();
			}
		}

		private void RemoveFeatures()
		{
			if (pFeatures2RemoveCount == 0)
				return;

			FeatureEntry[] newFeatures = new FeatureEntry[pFeatureEntries.Length - pFeatures2RemoveCount];
			int nextItemIdx = 0;
			foreach (var featureEntry in pFeatureEntries)
			{
				if (featureEntry.Removed)
					continue;

				newFeatures[nextItemIdx] = featureEntry;
				newFeatures[nextItemIdx].Index = nextItemIdx;

				++nextItemIdx;
			}

			pFeatureEntries = newFeatures;
			pFeatures2RemoveCount = 0;
		}

		private bool InsertFeatures()
		{
			if(pFeatures2Insert.Count == 0)
				return false;
			
			FeatureEntry[] newFeatures = new FeatureEntry[pFeatureEntries.Length + pFeatures2Insert.Count];
			// Copy Features to Feature Array
			Array.Copy(pFeatureEntries, newFeatures, pFeatureEntries.Length);

			int nextIdx = pFeatureEntries.Length;
			var nextNode = pFeatures2Insert.First;
			while(nextNode != null)
			{
				newFeatures[nextIdx] = nextNode.Value;
				nextNode.Value.InsertNode = null;

				nextNode = nextNode.Next;
				++nextIdx;
			}

			pFeatureEntries = newFeatures;
			pFeatures2Insert.Clear();
			return true;
		}
		private void SortFeatures()
		{
			Array.Sort(pFeatureEntries);
		}

		public IEnumerator<IRenderFeature> GetEnumerator()
		{
			var entries = pFeatureEntries;
			foreach(var entry in entries)
			{
				if (entry.Removed)
					continue;
				if (entry.Feature.IsDisposed)
				{
					lock (pSync)
						RemoveEntry(entry);
					continue;
				}
				yield return entry.Feature; 
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			lock (pSync)
			{
				foreach(var entry in pFeatureEntries)
				{
					if (!entry.Feature.IsDisposed)
						entry.Feature.Dispose();
				}

				var node = pFeatures2Insert.First;
				while(node != null)
				{
					if (!node.Value.Feature.IsDisposed)
						node.Value.Feature.Dispose();
				}

				pFeatures2Insert.Clear();
				pFeatureEntries = new FeatureEntry[0];
				pFeatures2RemoveCount = 0;

				pLookupFeatures.Clear();
			}
			GC.SuppressFinalize(this);
		}
	}
}
