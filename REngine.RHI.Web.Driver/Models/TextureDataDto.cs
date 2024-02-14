namespace REngine.RHI.Web.Driver.Models;

internal unsafe struct TextureDataDto
{
    public IntPtr Data;
    public IntPtr SrcBuffer;
    public ulong SrcOffset;
    public ulong Stride;
    public ulong DepthStride;

    public TextureDataDto()
    {
        Data = IntPtr.Zero;
        SrcBuffer = IntPtr.Zero;
        SrcOffset = Stride = DepthStride = 0;
    }

    public TextureDataDto(ITextureData data, IntPtr dataSource)
    {
        if(data.SrcBuffer is not null)
            dataSource = IntPtr.Zero;
        Data = dataSource;
        SrcBuffer = data.SrcBuffer?.Handle ?? IntPtr.Zero;
        SrcOffset = data.SrcOffset;
        Stride = data.Stride;
        DepthStride = data.DepthStride;
    }

    public ref TextureDataDto GetPinnableReference()
    {
        return ref this;
    }
}