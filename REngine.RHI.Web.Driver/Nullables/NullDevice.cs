namespace REngine.RHI.Web.Driver;

internal class NullDevice : IDevice
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
    
    public IBuffer CreateBuffer(in BufferDesc desc)
    {
        throw new NotImplementedException();
    }

    public IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
    {
        throw new NotImplementedException();
    }

    public IShader CreateShader(in ShaderCreateInfo createInfo)
    {
        throw new NotImplementedException();
    }

    public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
    {
        throw new NotImplementedException();
    }

    public IPipelineStateCache CreatePipelineStateCache()
    {
        throw new NotImplementedException();
    }

    public IPipelineStateCache CreatePipelineStateCache(byte[] initialData)
    {
        throw new NotImplementedException();
    }

    public ITexture CreateTexture(in TextureDesc desc)
    {
        throw new NotImplementedException();
    }

    public ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
    {
        throw new NotImplementedException();
    }

    public static readonly IDevice Instance = new NullDevice();
}