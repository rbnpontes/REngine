using REngine.Core;
using REngine.Core.Mathematics;
using REngine.Core.WorldManagement;

namespace REngine.RPI;

public sealed class BatchSystem : IDisposable
{
    private readonly object pSync = new();
    private readonly EngineEvents pEngineEvents;
    private readonly Dictionary<ulong, BatchGroup> pBatchGroups = new();
    
    private bool pDisposed;

    public BatchSystem(EngineEvents engineEvents)
    {
        pEngineEvents = engineEvents;
        engineEvents.OnBeforeStop.Once(HandleEngineStop);
    }

    private async Task HandleEngineStop(object sender)
    {
        await EngineGlobals.MainDispatcher.Yield();
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