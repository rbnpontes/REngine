namespace REngine.Core;

public sealed class RefCount<T>(T? instance) : IDisposable where T : IDisposable
{
    private bool pDisposed;
    
    public int Count { get; private set; } = instance is null ? 0 : 1;

    public T Ref
    {
        get
        {
            ObjectDisposedException.ThrowIf(pDisposed, instance);
            return instance;
        }
    }

    public void AddRef()
    {
        ++Count;
    }
    
    public void Dispose()
    {
        if (pDisposed)
            return;
        --Count;
        if (Count > 0)
            return;
        instance?.Dispose();
        instance = default(T);
        pDisposed = true;
    }

    public static RefCount<T> Empty = new (default(T));
}