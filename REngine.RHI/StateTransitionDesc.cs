namespace REngine.RHI;

public struct StateTransitionDesc
{
    public IGPUObject? ResourceBefore;
    public IGPUObject? Resource;
    public uint FirstMipLevel;
    public uint MipLevelsCount;
    public uint FirstArraySlice;
    public uint ArraySliceCount;
    public ResourceState OldState;
    public ResourceState NewState;
    public StateTransitionType TransitionType;
    public StateTransitionFlags Flags;

    public StateTransitionDesc()
    {
        ResourceBefore = Resource = null;
        FirstMipLevel = 0;
        MipLevelsCount = uint.MaxValue;
        FirstArraySlice = 0;
        ArraySliceCount = uint.MaxValue;
        OldState = ResourceState.Unknow;
        NewState = ResourceState.Unknow;
        TransitionType = StateTransitionType.Immediate;
        Flags = StateTransitionFlags.None;
    }

    public StateTransitionDesc(
        ITexture texture,
        ResourceState oldState,
        ResourceState newState,
        uint firstMipLevel = 0u,
        uint mipLevelsCount = uint.MaxValue,
        uint firstArraySlice = 0u,
        uint arraySliceCount = uint.MaxValue,
        StateTransitionType transitionType = StateTransitionType.Immediate,
        StateTransitionFlags flags = StateTransitionFlags.None)
    {
        Resource = texture;
        FirstMipLevel = firstMipLevel;
        MipLevelsCount = mipLevelsCount;
        FirstArraySlice = firstArraySlice;
        ArraySliceCount = arraySliceCount;
        OldState = oldState;
        NewState = newState;
        TransitionType = transitionType;
        Flags = flags;
    }

    public StateTransitionDesc(
        ITexture texture,
        ResourceState oldState,
        ResourceState newState,
        StateTransitionFlags flags)
    {
        Resource = texture;
        OldState = oldState;
        NewState = newState;
        FirstMipLevel = 0;
        MipLevelsCount = uint.MaxValue;
        FirstArraySlice = 0;
        ArraySliceCount = uint.MaxValue;
        TransitionType = StateTransitionType.Immediate;
        Flags = flags;
    }

    public StateTransitionDesc(
        IBuffer buffer,
        ResourceState oldState,
        ResourceState newState,
        StateTransitionFlags flags = StateTransitionFlags.None)
    {
        Resource = buffer;
        OldState = oldState;
        NewState = newState;
        Flags = flags;
        TransitionType = StateTransitionType.Immediate;
    }
}