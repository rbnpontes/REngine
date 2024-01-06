using System.Collections.Concurrent;
using REngine.RHI;

namespace REngine.RPI;

public static class GpuObjects
{
    private static readonly ConcurrentQueue<IGPUObject> pObj2Dispose = new();

    public static void AddToDispose(IGPUObject? obj)
    {
        if (obj is null)
            return;
        pObj2Dispose.Enqueue(obj);
    }

    public static void DisposeObjects()
    {
        while(pObj2Dispose.TryDequeue(out var obj))
            obj.Dispose();
    }
}