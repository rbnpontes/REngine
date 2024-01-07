using REngine.Assets;
using REngine.Core.Resources;
using REngine.Core.Threading;
using REngine.RHI;

namespace REngine.RPI.Resources;

public sealed class TextureAsset(IGraphicsDriver driver, IExecutionPipeline pipeline) : Asset
{
    private ITexture? pTexture;
    public ITexture Texture => pTexture ?? throw new NullReferenceException($"{nameof(TextureAsset)} not loaded");
    
    protected override void OnLoad(AssetStream stream)
    {
        using var asset = new ImageAsset();
        asset.Load(stream);
        var img = asset.Image;

        pTexture = driver.Device.CreateTexture(new TextureDesc()
        {
            Name = Name,
            AccessFlags = CpuAccessFlags.None,
            BindFlags = BindFlags.ShaderResource,
            Size = new TextureSize(img.Size.Width, img.Size.Height),
            Format = TextureFormat.RGBA8UNormSRGB,
            Usage = Usage.Immutable
        }, [new ByteTextureData(img.Data, img.Stride)]);
    }

    protected override void OnDispose()
    {
        pTexture?.Dispose();
        pTexture = null;
    }
}