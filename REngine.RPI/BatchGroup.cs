using System.Collections;
using REngine.Core.Utils;

namespace REngine.RPI;

public sealed class BatchGroup : IEnumerable<Batch>
{
    private readonly object pSync = new();
    private readonly Queue<int> pAvailableIndexes = new Queue<int>();
    private readonly List<Batch?> pBatches = [];

    private bool pIsLocked;

    public int NumBatches => pBatches.Count - pAvailableIndexes.Count;

    public void Lock()
    {
        Monitor.Enter(pSync);
        pIsLocked = true;
    }

    public void Unlock()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        pIsLocked = false;
        Monitor.Exit(pSync);
    }
    /// <summary>
    /// Insert a Batch into Group and return their index
    /// </summary>
    /// <param name="batch"></param>
    /// <returns></returns>
    public int AddBatch(Batch batch)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        int idx;
        if (pAvailableIndexes.Count == 0)
        {
            idx = pBatches.Count;
            pBatches.Add(batch);
        }
        else
        {
            idx = pAvailableIndexes.Dequeue();
            pBatches[idx] = batch;
        }

        return idx;
    }

    /// <summary>
    /// Remove a batch from group by given index
    /// </summary>
    /// <param name="batchIndex"></param>
    public void RemoveBatch(int batchIndex)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        ExceptionUtils.ThrowIfOutOfBounds(batchIndex, pBatches.Count);
        pBatches[batchIndex] = null;
        pAvailableIndexes.Enqueue(batchIndex);
    }

    /// <summary>
    /// Remove a batch from group by a Batch reference
    /// Always prefer RemoveBatch by index instead reference
    /// </summary>
    /// <param name="batch"></param>
    public void RemoveBatch(Batch batch)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        RemoveBatch(pBatches.IndexOf(batch));
    }

    /// <summary>
    /// Clear stored batches, but do not clear internal buffer
    /// </summary>
    public void Reset()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        pAvailableIndexes.Clear();
        for (var i = 0; i < pBatches.Count; ++i)
        {
            pAvailableIndexes.Enqueue(i);
            pBatches[i] = null;
        }
    }
    /// <summary>
    /// Discards internal buffer, and clear stored batches
    /// </summary>
    public void Clear()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        pBatches.Clear();
        pAvailableIndexes.Clear();
    }

    public Batch GetBatch(int batchIdx)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        ExceptionUtils.ThrowIfOutOfBounds(batchIdx, pBatches.Count);
        if (pAvailableIndexes.Count == pBatches.Count) 
            ThrowException();
        if(pBatches[batchIdx] is null)
            ThrowException();
        return pBatches[batchIdx] ?? throw new InvalidOperationException();

        void ThrowException()
        {
            throw new NullReferenceException("There's no allocated batch to given index");
        }
    }
    
    public void ForEach(Action<Batch> callback)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        if (pAvailableIndexes.Count == pBatches.Count)
            return;
        foreach (var batch in pBatches.OfType<Batch>())
            callback(batch);
    }
    public IEnumerator<Batch> GetEnumerator()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        if (pAvailableIndexes.Count == pBatches.Count)
            yield break;
        foreach (var batch in pBatches.OfType<Batch>())
            yield return batch;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void ValidateLock()
    {
        if (!pIsLocked)
            throw new Exception($"{nameof(BatchGroup)} must lock first");
    }
    
}