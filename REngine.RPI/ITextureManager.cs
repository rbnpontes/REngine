using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI;

public interface ITextureManager
{
    public ITexture Create(Image image, BindFlags flags = BindFlags.ShaderResource);
    public ITexture Create(TextureDesc desc);
    public ITexture Create(TextureDesc desc, ITextureData[] data);
}