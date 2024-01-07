using System.Collections;
using REngine.Core.Mathematics;
using REngine.Core.Utils;

namespace REngine.RPI;

public sealed class BatchGroup : IEnumerable<Batch>
{
    private readonly object pSync = new();
    private readonly Queue<int> pAvailableIndexes = new Queue<int>();
    private readonly List<Batch?> pBatches = [];

#if DEBUG
    private string pThreadLockName = string.Empty;
    private ulong pThreadId;
#endif
    private bool pIsLocked;
    private ulong pSortKey;
    
    public int NumBatches => pBatches.Count - pAvailableIndexes.Count;

    public void Lock()
    {
        Core.Threading.Monitor.Enter(pSync);
#if DEBUG
        pThreadLockName = Thread.CurrentThread.Name ?? "Unknown Thread";
        pThreadId = Hash.Digest(pThreadLockName);
#endif
        pIsLocked = true;
    }

    public void Unlock()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
#if DEBUG
        pThreadLockName = string.Empty;
        pThreadId = 0;
#endif
        pIsLocked = false;
        Core.Threading.Monitor.Exit(pSync);
    }
    /// <summary>
    /// Insert a Batch into Group and return their index
    /// </summary>
    /// <param name="batch"></param>
    /// <returns></returns>
    public Batch AddBatch(Batch batch)
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

        pSortKey = 0;
        batch.Id = idx;
        return batch;
    }

    /// <summary>
    /// Remove a batch from group by given index
    /// </summary>
    /// <param name="batchIndex"></param>
    public void RemoveBatch(int batchIndex)
    {
        if (batchIndex == -1)
            return;
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        ExceptionUtils.ThrowIfOutOfBounds(batchIndex, pBatches.Count);
        pBatches[batchIndex] = null;
        pSortKey = 0;
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
        RemoveBatch(batch.Id);
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

        pSortKey = 0;
    }
    /// <summary>
    /// Discards internal buffer, and clear stored batches
    /// </summary>
    public void Clear()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        pSortKey = 0;
        pBatches.Clear();
        pAvailableIndexes.Clear();
    }

    public void Sort()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        var sortKey = CalculateSortKey();
        if (sortKey == pSortKey)
            return;
        var batches = GetBatches();
        pAvailableIndexes.Clear();
        Array.Sort(batches);

        for (var i = 0; i < pBatches.Count; ++i)
        {
            if (i < batches.Length)
            {
                batches[i].Id = i;
                pBatches[i] = batches[i];
            }
            else
                pAvailableIndexes.Enqueue(i);
        }
        
        pSortKey = sortKey;
    }


    private ulong CalculateSortKey()
    {
        if (pAvailableIndexes.Count == pBatches.Count)
            return 0;
        var hash = 0ul;
        var numIterations = pBatches.Count - pAvailableIndexes.Count;
        var i = -1;
        while (numIterations != 0)
        {
            ++i;
            var batch = pBatches[i];
            if (batch is null || batch.IsDisposed)
                continue;

            --numIterations;
            hash = Hash.Combine(hash, (uint)batch.GetSortIndex());
        }

        return hash;
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
    
    public IEnumerator<Batch> GetEnumerator()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        if (pAvailableIndexes.Count == pBatches.Count)
            yield break;

        var numIterations = pBatches.Count - pAvailableIndexes.Count;
        var i = -1;
        while (numIterations != 0)
        {
            ++i;
            var batch = pBatches[i];
            if (batch is null || batch.IsDisposed)
                continue;

            --numIterations;
            yield return batch;
        }
    }

    public Batch[] GetBatches()
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        var batches = new Batch[pBatches.Count - pAvailableIndexes.Count];
        if (batches.Length == 0)
            return batches;
        
        var nextId = 0;
        foreach (var batch in pBatches.OfType<Batch>())
        {
            if(batch.IsDisposed)
                continue;
            batches[nextId] = batch;
            ++nextId;
            if (nextId == batches.Length)
                break;
        }

        return batches;
    }

    public void ForEach(Action<Batch> callback)
    {
#if RENGINE_VALIDATIONS
        ValidateLock();
#endif
        if (pAvailableIndexes.Count == pBatches.Count)
            return;

        var availableBatches = pBatches.Count - pAvailableIndexes.Count;
        var executedBatches = 0;
        var nextId = 0;
        
        while (executedBatches < availableBatches && nextId < pBatches.Count)
        {
            var batch = pBatches[nextId];
            ++nextId;
            if(batch is null)
              continue;

            callback(batch);
            ++executedBatches;
        }
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