namespace REngine.RHI.Web.Driver;

internal class NullPipelineState : IPipelineState
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.PipelineState;
    public string Name => "Null Pipeline State";
    public IShaderResourceBinding[] ShaderResourceBindings => [];
    public ulong ToHash() => 0;
    
    public void Dispose()
    {
    }
    
    public bool HasShaderResourceBinding(IShaderResourceBinding srb) => false;

    public IShaderResourceBinding GetResourceBinding() => NullShaderResourceBinding.Instance;

    public IShaderResourceBinding CreateResourceBinding() => NullShaderResourceBinding.Instance;

    public GraphicsPipelineDesc Desc => new ();
}
internal class NullComputePipelineState : IComputePipelineState
{
    public IntPtr Handle => IntPtr.Zero;
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.ComputePipeline;
    public ComputePipelineDesc Desc => new ();
    public string Name => "Null Compute Pipeline State";
    public IShaderResourceBinding[] ShaderResourceBindings => [];
    public ulong ToHash() => 0;

    public void Dispose()
    {
    }

    public bool HasShaderResourceBinding(IShaderResourceBinding srb) => false;

    public IShaderResourceBinding GetResourceBinding() => NullShaderResourceBinding.Instance;

    public IShaderResourceBinding CreateResourceBinding() => NullShaderResourceBinding.Instance;

    public static readonly NullComputePipelineState Instance = new();
}