using System.Runtime.CompilerServices;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class PipelineStateImpl(IntPtr handle, GraphicsPipelineDesc desc) : NativeObject(handle), IPipelineState
{
    private readonly Dictionary<IntPtr, IShaderResourceBinding> pSrbMap = new();
    public GPUObjectType ObjectType => GPUObjectType.GraphicsPipeline;
    public string Name => desc.Name;
    public IShaderResourceBinding[] ShaderResourceBindings => pSrbMap.Values.ToArray();
    public GraphicsPipelineDesc Desc => desc;
    
    public ulong ToHash() => desc.ToHash();

    public bool HasShaderResourceBinding(IShaderResourceBinding srb)
    {
        return pSrbMap.ContainsKey(srb.Handle);
    }

    private DefaultShaderResourceBindingImpl? pDefaultSrb;
    public IShaderResourceBinding GetResourceBinding()
    {
        if (pDefaultSrb is not null) 
            return pDefaultSrb;
        
        pDefaultSrb = new DefaultShaderResourceBindingImpl(AllocateSrb());
        pSrbMap[pDefaultSrb.Handle] = pDefaultSrb;
        return pDefaultSrb;
    }

    public IShaderResourceBinding CreateResourceBinding()
    {
        var srb = new ShaderResourceBindingImpl(
            AllocateSrb(),
            this);
        pSrbMap[srb.Handle] = srb;
        return srb;
    }

    internal void RemoveResourceBinding(IntPtr srb)
    {
        pSrbMap.Remove(srb);
    }
    private unsafe IntPtr AllocateSrb()
    {
        ResultNative result = new();
        var driverPtr = NativeApis.js_malloc(Unsafe.SizeOf<ResultNative>());
        NativeApis.js_memset(driverPtr, 0x0, Unsafe.SizeOf<ResultNative>());
        
        js_rengine_pipelinestate_createresourcebinding(Handle, driverPtr);
        fixed(void* ptr = result)
            NativeApis.js_memcpy(driverPtr, ptr, Unsafe.SizeOf<ResultNative>());
        NativeApis.js_free(driverPtr);

        if (result.Error != IntPtr.Zero)
            throw new DriverException(NativeApis.js_get_string(result.Error));
        if (result.Value == IntPtr.Zero)
            throw new NullReferenceException($"Could not possible to create {nameof(IShaderResourceBinding)}. Driver return null");
        return result.Value;
    }
}