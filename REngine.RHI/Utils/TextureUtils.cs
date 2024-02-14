using REngine.Core.Mathematics;

namespace REngine.RHI.Utils;

public static class TextureUtils
{
    public static bool IsTextureArray(in TextureDesc desc)
    {
        var dim = desc.Dimension;
        return dim is TextureDimension.Tex1DArray or TextureDimension.Tex2DArray or TextureDimension.Tex3DArray;
    }
    public static IntVector3 CalculateMipSize(in TextureDesc desc, int mipLevel)
    {
        mipLevel = (int)Math.Clamp(mipLevel, 0, desc.MipLevels);
        var result = IntVector3.Zero;
        switch (desc.Dimension)
        {
            case TextureDimension.Undefined:
            case TextureDimension.Buffer:
                throw new NotSupportedException($"Not supported texture dimension: {desc.Dimension}");
            case TextureDimension.Tex1D:
            case TextureDimension.Tex1DArray:
                result.X = (int)desc.Size.Width >> mipLevel;
                break;
            case TextureDimension.Tex2DArray:
            case TextureDimension.Tex2D:
                result.X = (int)desc.Size.Width >> mipLevel;
                result.Y = (int)desc.Size.Height >> mipLevel;
                break;
            case TextureDimension.Tex3D:
            case TextureDimension.Tex3DArray:
                result.X = (int)desc.Size.Width >> mipLevel;
                result.Y = (int)desc.Size.Height >> mipLevel;
                result.Z = (int)desc.ArraySizeOrDepth >> mipLevel;
                break;
        }

        return result;
    }
}