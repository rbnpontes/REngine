namespace REngine.RHI.NativeDriver;

internal partial class BufferViewImpl(IntPtr handle, IBuffer buffer) : NativeObject(handle), IBufferView
{
    public GPUObjectType ObjectType => GPUObjectType.BufferView;
    public string Name => string.Empty;
    public IBuffer Buffer => buffer;
}