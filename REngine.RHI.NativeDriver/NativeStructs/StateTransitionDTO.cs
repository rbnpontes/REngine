namespace REngine.RHI.NativeDriver.NativeStructs;

internal struct StateTransitionDTO
{
    public IntPtr ResourceBefore;
    public IntPtr Resource;
    public uint FirstMipLevel;
    public uint MipLevelsCount;
    public uint FirstArraySlice;
    public uint ArraySliceCount;
    public uint OldState;
    public uint NewState;
    public byte TransitionType;
    public byte Flags;

    public static void Fill(in StateTransitionDesc desc, out StateTransitionDTO output)
    {
        output.ResourceBefore = desc.ResourceBefore?.Handle ?? IntPtr.Zero;
        output.Resource = desc.Resource?.Handle ?? IntPtr.Zero;
        output.FirstMipLevel = desc.FirstMipLevel;
        output.MipLevelsCount = desc.MipLevelsCount;
        output.FirstArraySlice = desc.FirstArraySlice;
        output.ArraySliceCount = desc.ArraySliceCount;
        output.OldState = (uint)desc.OldState;
        output.NewState = (uint)desc.NewState;
        output.TransitionType = (byte)desc.TransitionType;
        output.Flags = (byte)desc.Flags;
    }
}