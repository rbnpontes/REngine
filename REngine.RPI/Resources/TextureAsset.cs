using REngine.Assets;
using REngine.Core.Resources;
using REngine.Core.Threading;
using REngine.RHI;

namespace REngine.RPI.Resources;

public sealed class TextureAsset(ITextureManager textureManager, IExecutionPipeline pipeline) : Asset
{
    private ITexture? pTexture;
    public ITexture Texture => pTexture ?? throw new NullReferenceException($"{nameof(TextureAsset)} not loaded");
    
    protected override void OnLoad(AssetStream stream)
    {
        using var asset = new ImageAsset();
        asset.Load(stream);
        var img = asset.Image;

        pTexture = textureManager.Create(img);
    }

    protected override void OnDispose()
    {
        pTexture?.Dispose();
        pTexture = null;
    }
}