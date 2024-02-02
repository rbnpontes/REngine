namespace REngine.RHI.Web.Driver;

internal static class ObjectRegistry
{
    private static readonly Dictionary<IntPtr, WeakReference<NativeObject>> sTrackingObjects = new();

    public static void Lock(NativeObject obj)
    {
        Lock(obj, obj.Handle);
    }

    public static void Lock(NativeObject obj, IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new NullReferenceException("Can´t lock object with IntPtr.Zero handle");
        
        if (sTrackingObjects.TryGetValue(handle, out WeakReference<NativeObject>? output))
        {
            output.SetTarget(obj);
            return;
        }
        
        sTrackingObjects.Add(handle, new WeakReference<NativeObject>(obj));
    }

    public static void Unlock(NativeObject obj)
    {
        Unlock(obj.Handle);
    }

    public static void Unlock(IntPtr ptr)
    {
        sTrackingObjects.Remove(ptr);
    }

    public static void ClearRegistry()
    {
        sTrackingObjects.Clear();
    }

    public static NativeObject? Acquire(IntPtr ptr)
    {
        NativeObject? obj = null;
        sTrackingObjects.TryGetValue(ptr, out var weakRef);
        weakRef?.TryGetTarget(out obj);
        return obj;
    }
}