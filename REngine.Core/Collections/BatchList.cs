using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Collections
{
	public class BatchList<T> : IList<T>
	{
		private T[] pBuffer;
		private uint pNextItemIdx;

		public T this[int index] 
		{
			get
			{
				ValidateIndex(index);
				T item = pBuffer[index];
				return item;
			} 
			set => pBuffer[index] = value;
		}

		public int Count => (int)pNextItemIdx;
		/// <summary>
		/// At each resize, the new Array will be [previousLength + GrowthSize]
		/// </summary>
		public uint GrowthSize { get; set; }

		public bool IsReadOnly => false;

		public int Allocated { get => pBuffer.Length; }

		public BatchList(uint initialSize, uint growthSize)
		{
			pBuffer = new T[initialSize];
			GrowthSize = growthSize;
			pNextItemIdx = 0;
		}

		public void Add(T item)
		{
			if (pNextItemIdx >= pBuffer.Length)
				RefitItems();
			pBuffer[pNextItemIdx++] = item;
		}

		public void Clear()
		{
			pNextItemIdx = 0;
		}
		public void Reset()
		{
			pBuffer = Array.Empty<T>();
			pNextItemIdx = 0;
		}

		public bool Contains(T item)
		{
			return pBuffer.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(pBuffer, 0, array, arrayIndex, pNextItemIdx);
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < pNextItemIdx; i++)
			{
				T? item = pBuffer[i];
				if (item is null)
					continue;
				yield return item;
			}
		}

		public int IndexOf(T item)
		{
			for(int i =0; i < pBuffer.Length; ++i)
			{
				if (Equals(pBuffer[i], item))
					return i;
			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			ValidateIndex(index);
			pBuffer[index] = item;
		}

		public bool Remove(T item)
		{
			return true;
		}

		public void RemoveAt(int index)
		{
		}

		public void Resize(uint length)
		{
			if (length == pBuffer.Length)
				return;
			var oldItems = pBuffer;
			pBuffer = new T[length];
			Array.Copy(oldItems, pBuffer, Math.Min(oldItems.Length, length));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < pNextItemIdx; ++i)
				yield return pBuffer[i];
		}

		private void ValidateIndex(int index)
		{
			if (index >= pNextItemIdx)
				throw new IndexOutOfRangeException("Index if greater than Count");
			if (index < 0)
				throw new IndexOutOfRangeException("Index is less than 0");
		}

		private void RefitItems()
		{
			var oldBuffer = pBuffer;
			pBuffer = new T[oldBuffer.Length + GrowthSize];
			Array.Copy(oldBuffer, pBuffer, oldBuffer.Length);
		}
	}
}
