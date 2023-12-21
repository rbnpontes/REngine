using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public abstract class BaseSystem<T> where T : struct
	{
		protected T[] pData;
		protected Queue<int> pAvailableIdx;

		public BaseSystem(int initialSize = 0)
		{
			pData = new T[initialSize];
			pAvailableIdx = new Queue<int>(initialSize);
			for(int i =0; i < initialSize; ++i)
				pAvailableIdx.Enqueue(i);
		}

		protected int Acquire()
		{
			if(pAvailableIdx.TryDequeue(out var id))
				return id;

			var expansionSize = GetExpansionSize();
			Expand(expansionSize);

			return Acquire();
		}

		protected void Expand(int newSlots)
		{
			newSlots = Math.Max(newSlots, 0);
			if (newSlots == 0)
				return;
			for (int i = 0; i < newSlots; ++i)
				pAvailableIdx.Enqueue(pData.Length + i);

			OnAllocate(pData.Length + newSlots);
		}

		protected virtual void OnAllocate(int requestSize)
		{
			requestSize = Math.Max(requestSize, 0);
			T[] newEntities = new T[requestSize];
			Array.Copy(pData, newEntities, Math.Min(requestSize, pData.Length));
			pData = newEntities;
		}

		protected virtual void ValidateId(int id)
		{
			if (id < 0 || id >= pData.Length)
				throw new IndexOutOfRangeException("Invalid ID");
		}

		protected abstract int GetExpansionSize();
	}
}
