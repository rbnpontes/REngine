using System.Runtime.CompilerServices;
using REngine.Core.Exceptions;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class DeviceImpl(IntPtr handle) : NativeObject(handle), IDevice
{
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

    public IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : struct
    {
        throw new NotImplementedException();
    }

    public IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
    {
        throw new NotImplementedException();
    }

    public unsafe IShader CreateShader(in ShaderCreateInfo createInfo)
    {
        if (createInfo.ByteCode.Length > 0)
            throw new NotSupportedException("Web Driver does not support shader creation from bytecode");
        if (string.IsNullOrEmpty(createInfo.SourceCode))
            throw new RequiredFieldException(typeof(ShaderCreateInfo), nameof(ShaderCreateInfo.SourceCode));
        
        var namePtr = string.IsNullOrEmpty(createInfo.Name)
            ? IntPtr.Zero
            : NativeApis.js_alloc_string(createInfo.Name);
        var sourceCodePtr = NativeApis.js_alloc_string(createInfo.SourceCode);
        var macroKeysPtr = createInfo.Macros.Count == 0
            ? IntPtr.Zero
            : NativeApis.js_malloc(NativeApis.js_get_ptr_size() * createInfo.Macros.Count);
        var macroValuesPtr = createInfo.Macros.Count == 0
            ? IntPtr.Zero
            : NativeApis.js_malloc(NativeApis.js_get_ptr_size() * createInfo.Macros.Count);
        var dataPtr = NativeApis.js_malloc(Unsafe.SizeOf<ShaderCreateInfoDto>());
        var resultPtr = NativeApis.js_malloc(Unsafe.SizeOf<ResultNative>());
        NativeApis.js_memset(resultPtr, 0, Unsafe.SizeOf<ResultNative>());
        
        List<IntPtr> pointers2Free = [
            namePtr, 
            sourceCodePtr, 
            macroKeysPtr, 
            macroValuesPtr,
            dataPtr,
            resultPtr
        ];
        
        for (var i = 0; i < createInfo.Macros.Count; ++i)
        {
            var macroKey = createInfo.Macros.Values.ElementAt(i);
            var macroValue = createInfo.Macros.Values.ElementAt(i);

            var macroKeyPtr = NativeApis.js_alloc_string(macroKey);
            var macroValuePtr = NativeApis.js_alloc_string(macroValue);
            
            NativeApis.js_write_i32(macroKeysPtr + i, macroKeyPtr.ToInt32());
            NativeApis.js_write_i32(macroValuesPtr + i, macroValuePtr.ToInt32());
            
            pointers2Free.Add(macroKeyPtr);
            pointers2Free.Add(macroValuePtr);
        }

        var dto = new ShaderCreateInfoDto(createInfo);
        dto.Name = namePtr;
        dto.SourceCode = sourceCodePtr;
        dto.MacroKeysPtr = macroKeysPtr;
        dto.MacroValuesPtr = macroValuesPtr;
        fixed(void* ptr = dto)
            NativeApis.js_memcpy(ptr, dataPtr, Unsafe.SizeOf<ShaderCreateInfoDto>());
        
        js_rengine_device_create_shader(handle, dataPtr, resultPtr);

        var result = new ResultNative();
        fixed(void* ptr = result)
            NativeApis.js_memcpy(resultPtr, ptr, Unsafe.SizeOf<ResultNative>());

        // Free Allocated Pointers
        foreach (var ptr in pointers2Free)
            NativeApis.js_free(ptr);

        if (result.Error != IntPtr.Zero)
            throw new DeviceException(NativeApis.js_get_string(result.Value));
        if (result.Value == IntPtr.Zero)
            throw new NullReferenceException("Driver returns null pointer");

        return new ShaderImpl(result.Value, createInfo);
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
}