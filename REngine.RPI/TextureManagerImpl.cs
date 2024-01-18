using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI;

internal class TextureManagerImpl(IServiceProvider provider) : ITextureManager
{
    private IGraphicsDriver? pDriver;

    private IGraphicsDriver GetDriver()
    {
        pDriver ??= provider.Get<IGraphicsDriver>();
        return pDriver;
    }
    
    public ITexture Create(Image image, BindFlags flags = BindFlags.ShaderResource)
    {
        return Create(new TextureDesc()
        {
            AccessFlags = CpuAccessFlags.None,
            BindFlags = flags,
            Size = new TextureSize(image.Size.Width, image.Size.Height),
            Format = TextureFormat.RGBA8UNormSRGB,
            Usage = Usage.Immutable
        }, [new ByteTextureData(image.Data, image.Stride)]);
    }

    public ITexture Create(TextureDesc desc)
    {
        return GetDriver().Device.CreateTexture(desc);
    }

    public ITexture Create(TextureDesc desc, ITextureData[] data)
    {
        return GetDriver().Device.CreateTexture(desc, data);
    }
}