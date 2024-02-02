using REngine.Core;

namespace REngine.RHI.Web.Driver;

internal partial class NativeObject : INativeObject
{
    private readonly IntPtr pReleaseCallbackPtr;
    public IntPtr Handle { get; private set; }
    public bool IsDisposed { get; private set; }
    public event EventHandler? OnDispose;

    public uint StrongRefs
    {
        get
        {
            if (Handle == IntPtr.Zero)
                return 0;
            return (uint)js_rengine_object_strongref_count(Handle);
        }
    }
    public uint WeakRefs
    {
        get
        {
            if (Handle == IntPtr.Zero)
                return 0;
            return (uint)js_rengine_object_weakref_count(Handle);
        }
    }
    
    
    public NativeObject(IntPtr handle)
    {
        Handle = handle;
        
        ObjectRegistry.Lock(this);
        pReleaseCallbackPtr = NativeApis.js_register_function(OnReleaseObject, "vi");
    }

    ~NativeObject()
    {
        Dispose(true);
    }
    
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool finalizing)
    {
        if (IsDisposed)
            return;
        IsDisposed = true;

        OnBeginDispose();
        var refs = Math.Max(StrongRefs, WeakRefs);

        IntPtr ptr = Handle;
        Handle = IntPtr.Zero;

        if (refs > 0)
        {
            js_rengine_object_release(ptr);
            NativeApis.js_unregister_function(pReleaseCallbackPtr);
        }
        
        OnDispose?.Invoke(this, EventArgs.Empty);
        OnEndDispose();
    }

    protected virtual void OnBeginDispose() {}
    protected virtual void OnEndDispose() {}

    internal void AddRef()
    {
        if(IsDisposed || Handle == IntPtr.Zero)
            return;
        js_rengine_object_addref(Handle);
    }
    private void OnReleaseObject()
    {
        if (Handle == IntPtr.Zero)
            return;
        ObjectRegistry.Unlock(this);
        Handle = IntPtr.Zero;
        
        OnDispose?.Invoke(this, EventArgs.Empty);
        NativeApis.js_unregister_function(pReleaseCallbackPtr);
    }
}