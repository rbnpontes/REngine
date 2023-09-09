using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Collections
{
	public class FixedList<T> : IEnumerable<T>, ICollection<T>, ICloneable
	{
		private T[] pData;
		public T this[int index] { 
			get => pData[index]; 
			set => pData[index] = value; 
		}

		public int Length => pData.Length;

		public int Count => pData.Length;

		public bool IsReadOnly => false;

		public FixedList(int size)
		{
			pData = new T[size];
		}
		public FixedList(T[] data)
		{
			pData = (T[])data.Clone();
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach(var item in pData)
				yield return item;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			for (int i =0; i < pData.Length; ++i)
			{
				if (pData[i] == null)
				{
					pData[i] = item;
					return;
				}
			}
#pragma warning restore CS8602 // Dereference of a possibly null reference.

			throw new Exception("Can't add item to FixedList because is already full");
		}

		public void Clear()
		{
			Array.Fill(pData, default(T));
		}

		public bool Contains(T item)
		{
			return pData.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(pData, arrayIndex, array, arrayIndex, pData.Length);
		}

		public bool Remove(T item)
		{
			for(int i =0; i < pData.Length; ++i)
			{
				if (Equals(pData[i], item))
				{
#pragma warning disable CS8601 // Possible null reference assignment.
					pData[i] = default(T);
#pragma warning restore CS8601 // Possible null reference assignment.
					return true;
				}
			}
			return false;
		}

		public object Clone()
		{
			return new FixedList<T>(pData);
		}
	}
}
