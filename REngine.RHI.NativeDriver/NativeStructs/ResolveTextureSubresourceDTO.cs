namespace REngine.RHI.NativeDriver.NativeStructs;

public struct ResolveTextureSubresourceDTO
{
    public uint SrcMipLevel;
    public uint SrcSlice;
    public byte SrcTextureTransitionMode;
    public uint DstMipLevel;
    public uint DstSlice;
    public byte DstTextureTransitionMode;
    public ushort Format;

    public static void Fill(in ResolveTextureSubresourceDesc desc, out ResolveTextureSubresourceDTO output)
    {
        output.SrcMipLevel = desc.SrcMipLevel;
        output.SrcSlice = desc.SrcSlice;
        output.SrcTextureTransitionMode = (byte)desc.SrcTextureTransitionMode;
        output.DstMipLevel = desc.DstMipLevel;
        output.DstSlice = desc.DstSlice;
        output.DstTextureTransitionMode = (byte)desc.DstTextureTransitionMode;
        output.Format = (ushort)desc.Format;
    }
}