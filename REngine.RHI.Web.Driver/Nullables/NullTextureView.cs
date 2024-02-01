namespace REngine.RHI.Web.Driver;

internal class NullTextureView : ITextureView
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.TextureView;
    public string Name => "Null Texture View";
    public ITexture Parent => NullTexture.Instance;
    public TextureViewDesc Desc => new TextureViewDesc();
    public TextureViewType ViewType => TextureViewType.ShaderResource;
    public TextureSize Size => new TextureSize();
    
    public void Dispose()
    {
    }

    public static readonly NullTextureView Instance = new();
}