using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Collections
{
	public class EntityPool<T> : IEnumerable<T>
	{
		private int pCount = 0;
		private List<T?> pEntities = new List<T?>();
		private Queue<int> pAvailableIds = new Queue<int>();

		public int Count => pCount;

		public bool IsReadOnly => false;

		public int Add(T entity)
		{
			int idx;
			if(pAvailableIds.Count == 0)
			{
				idx = pEntities.Count;
				pEntities.Add(entity);
			} 
			else
			{
				idx = pAvailableIds.Dequeue();
				pEntities[idx] = entity;
			}
			++pCount;
			return idx;
		}

		public void Remove(T entity)
		{
			RemoveAt(pEntities.IndexOf(entity));
		}
		public void RemoveAt(int id)
		{
			if (id >= pEntities.Count)
				throw new Exception($"Invalid Entity Id. Id is greater than entities list ({id} >= {pEntities.Count}).");
			if (id < 0)
				throw new Exception($"Invalid Entity Id. Id is negative value. ({id})");

			pEntities[id] = default(T);
			pAvailableIds.Enqueue(id);
			--pCount;
		}

		public int GetEntityId(T entity)
		{
			return pEntities.IndexOf(entity);
		}

		public T? GetEntity(int id)
		{
			if (pEntities.Count >= id || id < 0)
				return default(T);
			return pEntities[id];
		}

		public bool Contains(T item)
		{
			return pEntities.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			int length = Math.Min(array.Length, pEntities.Count);

			int nextIdx = arrayIndex;
			for(int i =0;i < pEntities.Count; ++i)
			{
				if (nextIdx >= length)
					break;
				var entity = pEntities[i];
				if (entity != null)
				{
					array[nextIdx] = entity;
					++nextIdx;
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach(var item in pEntities)
			{
				if (item == null)
					continue;
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
