using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Utils;
using REngine.RHI;
using REngine.RHI.Utils;

namespace REngine.RPI;

internal class ShaderResourceBindingCacheImpl(
    IPipelineStateManager pipelineStateManager,
    ILoggerFactory loggerFactory
) : IShaderResourceBindingCache
{
    private readonly Dictionary<ulong, ShaderResourceBindingWrapper> pResourceBindings = new();
    private readonly ILogger<IShaderResourceBindingCache> pLogger = loggerFactory.Build<IShaderResourceBindingCache>();

    private class ShaderResourceBindingWrapper(
        ulong hash,
        IShaderResourceBinding srb,
        ShaderResourceBindingCacheImpl impl) : IShaderResourceBinding
    {
        private int pRefs = 1;
        public IntPtr Handle
        {
            get
            {
#if DEBUG
                ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
                return srb.Handle;
            }
        }
        public bool IsDisposed { get; private set; }
        public event EventHandler? OnDispose;

        public void Dispose()
        {
            if (IsDisposed)
                return;
            --pRefs;
            if (pRefs > 0)
                return;

            IsDisposed = true;
            OnDispose?.Invoke(this, EventArgs.Empty);
            srb.Dispose();
            impl.pResourceBindings.Remove(hash);
        }

        public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
        {
#if DEBUG
            ObjectDisposedException.ThrowIf(IsDisposed, this);
#endif
            srb.Set(flags, resourceName, resource);
        }

        public void AddRef()
        {
            pRefs++;
        }
    }

    public IShaderResourceBindingCache Clear()
    {
        pLogger.Debug($"Clearing ({pResourceBindings.Count}) Shader Resource Bindings.");
        foreach (var pair in pResourceBindings)
        {
            while(!pair.Value.IsDisposed)
                pair.Value.Dispose();
        }
        pResourceBindings.Clear();
        return this;
    }

    public IShaderResourceBinding Build(ulong pipelineHash, ResourceMapping resourceMapping)
    {
        var hash = Hash.Combine(pipelineHash, resourceMapping.ToHash());
        if (pResourceBindings.TryGetValue(hash, out var srb))
        {
            srb.AddRef();
            return srb;
        }
        
        var pipelineState = pipelineStateManager.FindGraphicsPipelineByHash(pipelineHash);
        if (pipelineState is null)
            throw new NullReferenceException($"Not found pipeline with hash #{pipelineHash}");

        pLogger.Debug($"Building #{pipelineHash} Shader Resource Binding.");
        var resources = resourceMapping.GetEntries();
        var target = pipelineState.CreateResourceBinding();

        srb = new ShaderResourceBindingWrapper(
            hash,
            target,
            this
        );
        
        foreach (var resource in resources)
        {
            var shaderFlags = EnumUtils.GetShaderTypeFlags(resource.Type);
            srb.Set(shaderFlags, resource.Name, resource.GPUObject);
        }

        pResourceBindings.Add(hash, srb);
        return srb;
    }

    public IShaderResourceBinding Build(IPipelineState pipelineState, ResourceMapping resourceMapping)
    {
        return Build(pipelineState.ToHash(), resourceMapping);
    }
}