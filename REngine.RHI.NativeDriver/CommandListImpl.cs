namespace REngine.RHI.NativeDriver;

internal class CommandListImpl(IntPtr handle) : ICommandList
{
    public GPUObjectType ObjectType => GPUObjectType.Unknown;
    public string Name => nameof(ICommandList);

    public IntPtr Handle => handle;
    public bool IsDisposed { get; private set; }
    public event EventHandler? OnDispose;
    
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        OnDispose?.Invoke(this, EventArgs.Empty);
        NativeObject.rengine_object_releaseref(Handle);
    }
}
