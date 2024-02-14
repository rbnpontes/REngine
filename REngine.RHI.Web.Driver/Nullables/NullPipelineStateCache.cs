namespace REngine.RHI.Web.Driver;

internal class NullPipelineStateCache : IPipelineStateCache
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.PipelineStateCache;
    public string Name => "Null Pipeline State Cache";
    public void GetData(out byte[] data)
    {
        data = [];
    }
    public void Dispose()
    {
    }

    public static readonly NullPipelineStateCache Instance = new ();
}