namespace REngine.RHI.Web.Driver;

internal class PipelineStateImpl(IntPtr handle, GraphicsPipelineDesc desc) : NativeObject(handle), IPipelineState
{
    public GPUObjectType ObjectType => GPUObjectType.GraphicsPipeline;
    public string Name => desc.Name;
    public IShaderResourceBinding[] ShaderResourceBindings => [];
    public GraphicsPipelineDesc Desc => desc;
    
    public ulong ToHash() => desc.ToHash();

    public bool HasShaderResourceBinding(IShaderResourceBinding srb)
    {
        throw new NotImplementedException();
    }

    public IShaderResourceBinding GetResourceBinding()
    {
        throw new NotImplementedException();
    }

    public IShaderResourceBinding CreateResourceBinding()
    {
        throw new NotImplementedException();
    }

}