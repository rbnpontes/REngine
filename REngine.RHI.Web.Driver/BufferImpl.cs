namespace REngine.RHI.Web.Driver;

internal partial class BufferImpl(IntPtr handle, BufferDesc desc) : NativeObject(handle), IBuffer
{
    public GPUObjectType ObjectType => GPUObjectType.Buffer;
    public string Name => desc.Name;

    public ResourceState State
    {
        get => (ResourceState)js_rengine_buffer_get_state(Handle);
        set => js_rengine_buffer_set_state(Handle, (int)value);
    }

    public ulong GPUHandle => (ulong)js_rengine_buffer_get_gpuhandle(Handle);
    public BufferDesc Desc => desc;
    public ulong Size => desc.Size;
    
    public IBufferView GetDefaultView(BufferViewType viewType)
    {
        throw new NotImplementedException();
    }

    public IBufferView CreateView(BufferViewDesc desc)
    {
        throw new NotImplementedException();
    }
}