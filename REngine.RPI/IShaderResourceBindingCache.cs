using REngine.RHI;

namespace REngine.RPI;

public interface IShaderResourceBindingCache
{
    public IShaderResourceBindingCache Clear();
    public IShaderResourceBinding Build(ulong pipelineHash, ResourceMapping resourceMapping);
    public IShaderResourceBinding Build(IPipelineState pipelineState, ResourceMapping resourceMapping);
}