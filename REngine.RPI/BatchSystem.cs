using REngine.Core;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;

namespace REngine.RPI;

public sealed class BatchSystem : IDisposable
{
    private readonly object pSync = new object();
    private readonly EngineEvents pEngineEvents;
    private readonly Dictionary<ulong, BatchGroup> pBatchGroups = new Dictionary<ulong, BatchGroup>();
    
    private bool pDisposed;

    public BatchSystem(EngineEvents engineEvents)
    {
        pEngineEvents = engineEvents;
        engineEvents.OnBeforeStop +=HandleEngineStop;
    }

    private void HandleEngineStop(object? sender, EventArgs e)
    {
        pEngineEvents.OnBeforeStop -= HandleEngineStop;
        Dispose();
    }

    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        lock (pBatchGroups)
        {
            pBatchGroups.Clear();
        }
    }

    public BatchGroup GetGroup(ulong groupHash)
    {
        ObjectDisposedException.ThrowIf(pDisposed, this);
        lock (pSync)
        {
            if (pBatchGroups.TryGetValue(groupHash, out var grp))
                return grp;
            grp = new BatchGroup();
            pBatchGroups[groupHash] = grp;
            return grp;
        }
    }
    public BatchGroup GetGroup(string groupName) => GetGroup(Hash.Digest(groupName));
}