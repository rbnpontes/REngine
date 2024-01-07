namespace REngine.RHI;

public struct ResolveTextureSubresourceDesc
{
    public uint SrcMipLevel;
    public uint SrcSlice;
    public ResourceStateTransitionMode SrcTextureTransitionMode;
    public uint DstMipLevel;
    public uint DstSlice;
    public ResourceStateTransitionMode DstTextureTransitionMode;
    public TextureFormat Format;
}