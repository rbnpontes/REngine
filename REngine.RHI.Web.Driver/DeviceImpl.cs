using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using REngine.Core.Exceptions;
using REngine.Core.IO;
using REngine.Core.Serialization;
using REngine.RHI.Utils;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class DeviceImpl(IntPtr handle, ILogger<IDevice> logger) : NativeObject(handle), IDevice
{
#if RENGINE_VALIDATIONS
    private void ValidateBufferDesc(in BufferDesc desc)
    {
        if((desc.BindFlags & BindFlags.UniformBuffer) != 0)
            ValidateUniformBuffer(desc);
    }
    private void ValidateUniformBuffer(in BufferDesc desc)
    {
        if ((desc.BindFlags & BindFlags.VertexBuffer) != 0 || (desc.BindFlags & BindFlags.IndexBuffer) != 0)
            throw new Exception(
                "Is not possible to create a Uniform Buffer with VertexBuffer or IndexBuffer bind flags");
    }
#endif
    public IBuffer CreateBuffer(in BufferDesc desc) => CreateBuffer(desc, IntPtr.Zero, 0);

    public unsafe IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged
    {
        var data = (values ?? []).ToArray().AsSpan();
        fixed(void* ptr = data)
            return CreateBuffer(desc, new IntPtr(ptr), (ulong)(data.Length * Unsafe.SizeOf<T>()));
    }

    public unsafe IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged
    {
        fixed (void* ptr = values)
            return CreateBuffer(desc, new IntPtr(ptr), (ulong)(values.Length * Unsafe.SizeOf<T>()));
    }

    public unsafe IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : unmanaged
    {
        T[] dataArr = [data];
        fixed(void* ptr = dataArr)
            return CreateBuffer(desc, new IntPtr(ptr), (ulong)Unsafe.SizeOf<T>());
    }

    public unsafe IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
    {
#if RENGINE_VALIDATIONS
        ValidateBufferDesc(desc);
#endif
        var resultSize = Unsafe.SizeOf<ResultNative>();
        var descSize = Unsafe.SizeOf<BufferDescDto>();
        var totalMemory = resultSize + descSize;
        
        if (data != IntPtr.Zero)
            totalMemory += (int)size;

        var resultPtr = NativeApis.js_malloc(totalMemory);
        var descPtr = resultPtr + resultSize;
        var dataPtr = descPtr + descSize;
        NativeApis.js_memset(resultPtr, 0x0, totalMemory);

        if (data == IntPtr.Zero)
            dataPtr = IntPtr.Zero;
        
        var result = new ResultNative();
        var descDto = new BufferDescDto(desc);
        fixed(void* ptr = descDto)
            NativeApis.js_memcpy(ptr, descPtr, descSize);
        if(data != IntPtr.Zero)
            NativeApis.js_memcpy(data.ToPointer(), dataPtr, (int)size);
        
        js_rengine_device_create_buffer(Handle, descPtr, (int)size, dataPtr, resultPtr);
        fixed(void* ptr = result)
            NativeApis.js_memcpy(resultPtr, ptr, resultSize);
            
        NativeApis.js_free(resultPtr);
        NativeApis.js_free(descDto.Name);

        if (result.Error != IntPtr.Zero)
            throw new DeviceException(NativeApis.js_get_string(result.Error));
        if (result.Value == IntPtr.Zero)
            throw new NullReferenceException(
                $"Could not possible to create {nameof(IBuffer)} object. Device returns null");
        return new BufferImpl(result.Value, desc);
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

    public unsafe IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
    {
        var inputLayoutSize = Unsafe.SizeOf<InputLayoutElementDescDto>() * desc.InputLayouts.Count;
        var immutableSamplerSize = Unsafe.SizeOf<ImmutableSamplerDescDto>() * desc.Samplers.Count;
        var renderTargetFormatsSize = sizeof(int) * desc.Output.RenderTargetFormats.Count;
        var descSize = Unsafe.SizeOf<GraphicsPipelineDescDto>();
        var resultSize = Unsafe.SizeOf<ResultNative>();
        var totalMemory = inputLayoutSize + immutableSamplerSize + renderTargetFormatsSize + descSize + resultSize;
        // Allocate an single block of memory
        // This can improve efficiency when CPU is reading data
        var inputLayoutsPtr = NativeApis.js_malloc(totalMemory);
        var immutableSamplersPtr = inputLayoutsPtr + inputLayoutSize;
        var rtFormatsPtr = immutableSamplersPtr + immutableSamplerSize;
        var descPtr = rtFormatsPtr + renderTargetFormatsSize;
        var resultPtr = descPtr + descSize;
        
        NativeApis.js_memset(inputLayoutsPtr, 0x0, totalMemory);
        if(inputLayoutSize == 0)
            inputLayoutsPtr = IntPtr.Zero;
        if(immutableSamplerSize == 0)
            immutableSamplersPtr = IntPtr.Zero;
        if(renderTargetFormatsSize == 0)
            rtFormatsPtr = IntPtr.Zero;
        
        
        var inputLayouts = desc.InputLayouts.Select(x => new InputLayoutElementDescDto(x)).ToArray();
        var immutableSamplers = desc.Samplers.Select(x =>
        {
            var str = NativeApis.js_alloc_string(x.Name);
            return new ImmutableSamplerDescDto(x) { Name = str };
        }).ToArray();
        var rtFormats = desc.Output.RenderTargetFormats.Select(x => (int)x).ToArray();
        var dto = new GraphicsPipelineDescDto(desc)
        {
            Name = NativeApis.js_alloc_string(desc.Name),
            InputLayouts = inputLayoutsPtr,
            ImmutableSamplers = immutableSamplersPtr,
            Output_RtFormats = rtFormatsPtr
        };
        
        fixed(void* ptr = inputLayouts)
            NativeApis.js_memcpy(ptr, inputLayoutsPtr, inputLayoutSize);
        fixed(void* ptr = immutableSamplers)
            NativeApis.js_memcpy(ptr, immutableSamplersPtr, immutableSamplerSize);
        fixed(void* ptr = rtFormats)
            NativeApis.js_memcpy(ptr, rtFormatsPtr, renderTargetFormatsSize);
        fixed(void* ptr = dto)
            NativeApis.js_memcpy(ptr, descPtr, descSize);
        
        js_rengine_device_create_graphicspipeline(Handle, descPtr, 0x1, resultPtr);
        ResultNative result = new();
        fixed(void* ptr = result)
            NativeApis.js_memcpy(resultPtr, ptr, resultSize);
        NativeApis.js_free(inputLayoutsPtr);
        NativeApis.js_free(dto.Name);
        foreach(var sampler in immutableSamplers)
            NativeApis.js_free(sampler.Name);
        
        if (result.Error != IntPtr.Zero)
            throw new DriverException(NativeApis.js_get_string(result.Error));
        if (result.Value == IntPtr.Zero)
            throw new NullReferenceException("Could not possible to create Pipeline State. Driver returns null pointer");
        return new PipelineStateImpl(result.Value, desc);
    }

    public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
    {
        logger.Warning("Compute Pipeline State is not supported on WebGL");
        return NullComputePipelineState.Instance;
    }

    public IPipelineStateCache CreatePipelineStateCache()
    {
        logger.Warning("Pipeline State Cache is not supported on WebGL");
        return NullPipelineStateCache.Instance;
    }

    public IPipelineStateCache CreatePipelineStateCache(byte[] initialData)
    {
        logger.Warning("Pipeline State Cache is not supported on WebGL");
        return NullPipelineStateCache.Instance;
    }

    public ITexture CreateTexture(in TextureDesc desc)
    {
        return CreateTexture(desc, []);
    }

    public unsafe ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
    {
        var texDescSize = Unsafe.SizeOf<TextureDescDto>();
        var texDataSize = Unsafe.SizeOf<TextureDataDto>();
        var resultSize = Unsafe.SizeOf<ResultNative>();
        
        var textureDatas = subresources as ITextureData[] ?? subresources.ToArray();
        
        var totalMemory = 0;
        
        if (desc.Usage is Usage.Immutable or Usage.Default && (desc.BindFlags & BindFlags.RenderTarget) == 0)
        {
            if (TextureUtils.IsTextureArray(desc))
            {
                if (desc.MipLevels * desc.ArraySizeOrDepth != textureDatas.Length)
                    throw new DeviceException(
                        $"MipLevels or ArraySize or Depth does not match SubResources Count. MipLevels={desc.MipLevels}, ArraySizeOrDepth={desc.ArraySizeOrDepth}, SubResources={textureDatas.Length}");

                for (var i = 0; i < textureDatas.Length; ++i)
                {
                    var mipSize = TextureUtils.CalculateMipSize(desc, i % (int)desc.MipLevels);
                    var size = mipSize.Y * mipSize.Z * (int)textureDatas[i].Stride;
                    if (textureDatas[i].Data != IntPtr.Zero)
                        totalMemory += size;
                }
            }
            else
            {
                if (textureDatas.Length != desc.MipLevels)
                    throw new DeviceException(
                        $"MipLevels Count does not match SubResources Count. MipLevels={desc.MipLevels}, SubResources={textureDatas.Length}");

                for (var i = 0; i < textureDatas.Length; ++i)
                {
                    var mipSize = TextureUtils.CalculateMipSize(desc, i);
                    var size = mipSize.Y * mipSize.Z * (int)textureDatas[i].Stride;
                    if (textureDatas[i].Data != IntPtr.Zero)
                        totalMemory += size;
                }
            }
        }

        totalMemory += texDescSize;
        totalMemory += texDataSize * textureDatas.Length;
        totalMemory += resultSize;

        // Allocate enough memory on driver side
        // Then copy all data into this memory and creates Texture handle
        var memData = NativeApis.js_malloc(totalMemory);
        NativeApis.js_memset(memData, 0x0, totalMemory);
        
        var resultPtr = memData;
        var texDescPtr = resultPtr + Unsafe.SizeOf<ResultNative>();
        var subResourcesPtr = texDescPtr + Unsafe.SizeOf<TextureDescDto>();
        var resourcesDataPtr = subResourcesPtr + Unsafe.SizeOf<TextureDataDto>() * textureDatas.Length;

        if (textureDatas.Length == 0)
            subResourcesPtr = resourcesDataPtr = IntPtr.Zero;
        
        var texDto = new TextureDescDto(desc);
        // Copy Desc Data
        fixed(void* ptr = texDto)
            NativeApis.js_memcpy(ptr, texDescPtr, texDescSize);

        var nextResourceStructPtr = subResourcesPtr;
        var nextResourceDataPtr = resourcesDataPtr;
        
        for (var i = 0; i < textureDatas.Length; ++i)
        {
            var mipSize = TextureUtils.CalculateMipSize(desc, i % (int)desc.MipLevels);
            var texData = new TextureDataDto(textureDatas[i], nextResourceDataPtr);
            fixed(void* ptr = texData)
                NativeApis.js_memcpy(ptr, nextResourceStructPtr, texDataSize);

            nextResourceStructPtr += texDataSize;
            if(textureDatas[i].SrcBuffer is not null)
                continue;

            var size = mipSize.Y * mipSize.Z * (int)textureDatas[i].Stride;
            // Copy Data to Driver Data
            NativeApis.js_memcpy(textureDatas[i].Data.ToPointer(), nextResourceDataPtr, size);
            nextResourceDataPtr += size;
        }
        
        var result = new ResultNative();
        js_rengine_device_create_texture(
            Handle,
            texDescPtr,
            resourcesDataPtr,
            textureDatas.Length,
            resultPtr);

        fixed(void* ptr = result)
            NativeApis.js_memcpy(resultPtr, ptr, resultSize);
        NativeApis.js_free(memData);
        NativeApis.js_free(texDto.Name);

        if (result.Error != IntPtr.Zero)
            throw new DeviceException(NativeApis.js_get_string(result.Error));
        if (result.Value == IntPtr.Zero)
            throw new NullReferenceException(
                $"Could not possible to create {nameof(ITexture)} object. Device returned null.");
        
        return new TextureImpl(result.Value);
    }
}