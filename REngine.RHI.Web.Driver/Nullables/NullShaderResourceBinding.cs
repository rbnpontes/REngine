namespace REngine.RHI.Web.Driver;

internal class NullShaderResourceBinding : IShaderResourceBinding
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    
    public void Dispose()
    {
    }
    public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
    {
    }

    public static readonly NullShaderResourceBinding Instance = new();
}