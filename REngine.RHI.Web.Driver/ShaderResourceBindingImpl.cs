namespace REngine.RHI.Web.Driver;

internal partial class ShaderResourceBindingImpl(IntPtr handle, PipelineStateImpl pipelineState) : NativeObject(handle), IShaderResourceBinding
{
    protected override void OnBeginDispose()
    {
        pipelineState.RemoveResourceBinding(handle);
    }

    public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
    {
        SetResource(handle, flags, resourceName, resource);
    }

    public static void SetResource(IntPtr srb, ShaderTypeFlags flags, string resName, IGPUObject res)
    {
        var resNamePtr = NativeApis.js_alloc_string(resName);
        
        if (TestFlags(flags, ShaderTypeFlags.Vertex))
            SetShaderRes((int)ShaderTypeFlags.Vertex, resNamePtr, res);
        if(TestFlags(flags, ShaderTypeFlags.Pixel))
            SetShaderRes((int)ShaderTypeFlags.Pixel, resNamePtr, res);
        if(TestFlags(flags, ShaderTypeFlags.Compute))
            SetShaderRes((int)ShaderTypeFlags.Compute, resNamePtr, res);
        if(TestFlags(flags, ShaderTypeFlags.Domain))
            SetShaderRes((int)ShaderTypeFlags.Domain, resNamePtr, res);
        if(TestFlags(flags, ShaderTypeFlags.Hull))
            SetShaderRes((int)ShaderTypeFlags.Hull, resNamePtr, res);
        if(TestFlags(flags, ShaderTypeFlags.Geometry))
            SetShaderRes((int)ShaderTypeFlags.Geometry, resNamePtr, res);
        
        NativeApis.js_free(resNamePtr);
        return;
        bool TestFlags(ShaderTypeFlags flags, ShaderTypeFlags expected)
        {
            return (flags & expected) != 0;
        }
        
        void SetShaderRes(int type, IntPtr resourceNamePtr, IGPUObject resource)
        {
            js_rengine_srb_set(srb, type, resourceNamePtr, resource.Handle);
        }
    }
}

internal class DefaultShaderResourceBindingImpl(IntPtr handle) : UndisposableNativeObject(handle), IShaderResourceBinding
{
    public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
    {
        ShaderResourceBindingImpl.SetResource(handle, flags, resourceName, resource);
    }
}