namespace REngine.RHI.Web.Driver;

internal class NullTexture : ITexture
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.Texture2D;
    public string Name => "Null Texture";
    public ResourceState State { get; set; }
    public ulong GPUHandle => 0;
    public TextureDesc Desc => new TextureDesc();
    
    public void Dispose()
    {
    }
    public ITextureView GetDefaultView(TextureViewType view)
    {
        return NullTextureView.Instance;
    }

    public static readonly NullTexture Instance = new();
}