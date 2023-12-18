using REngine.Core.IO;
using REngine.RHI;
using REngine.RHI.Utils;

namespace REngine.RPI;

internal class ShaderResourceBindingCacheImpl(
    IPipelineStateManager pipelineStateManager,
    ILoggerFactory loggerFactory
    ) : IShaderResourceBindingCache
{
    private readonly Dictionary<ulong, IShaderResourceBinding> pResourceBindings = new();
    private readonly ILogger<IShaderResourceBindingCache> pLogger = loggerFactory.Build<IShaderResourceBindingCache>();
    
    public IShaderResourceBindingCache Clear()
    {
        pLogger.Debug($"Clearing ({pResourceBindings.Count}) Shader Resource Bindings.");
        foreach (var pair in pResourceBindings)    
            pair.Value.Dispose();
        pResourceBindings.Clear();
        return this;
    }

    public IShaderResourceBinding Build(ulong pipelineHash, ResourceMapping resourceMapping)
    {
        if (pResourceBindings.TryGetValue(pipelineHash, out var srb))
            return srb;
        var pipelineState = pipelineStateManager.FindGraphicsPipelineByHash(pipelineHash);
        if (pipelineState is null)
            throw new NullReferenceException($"Not found pipeline with hash #{pipelineHash}");
        
        pLogger.Debug($"Building #{pipelineHash} Shader Resource Binding.");
        var resources = resourceMapping.GetEntries();
        srb = pipelineState.CreateResourceBinding();

        foreach (var resource in resources)
        {
            var shaderFlags = EnumUtils.GetShaderTypeFlags(resource.Type);
            srb.Set(shaderFlags, resource.Name, resource.GPUObject);
        }

        pResourceBindings.Add(pipelineHash, srb);
        return srb;
    }

    public IShaderResourceBinding Build(IPipelineState pipelineState, ResourceMapping resourceMapping)
    {
        return Build(pipelineState.ToHash(), resourceMapping);
    }
}