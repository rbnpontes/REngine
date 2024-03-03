using System.Runtime.CompilerServices;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class TextureImpl : NativeObject, ITexture
{
    private readonly TextureViewImpl?[] pTexViews = new TextureViewImpl?[(byte)TextureViewType.ShadingRate];
    public GPUObjectType ObjectType { get; }
    public string Name => Desc.Name;

    public ResourceState State
    {
        get => (ResourceState)js_rengine_texture_get_state(Handle);
        set => js_rengine_texture_set_state(Handle, (int)value);
    }
    public ulong GPUHandle => (ulong)js_rengine_texture_get_gpuhandle(Handle).ToInt64();
    public TextureDesc Desc { get; }
    
    public TextureImpl(IntPtr handle) : base(handle)
    {
        GetObjectDesc(handle, out var desc);
        ObjectType = GetObjectTypeFromDesc(desc);
        Desc = desc;
    }

    protected override void OnBeginDispose()
    {
        foreach (var texView in pTexViews)
        {
            if(texView is null)
                continue;
            if(!texView.IsDisposed)
                texView.Dispose();
        }
        
        Array.Fill(pTexViews, null);
    }

    public unsafe ITextureView GetDefaultView(TextureViewType view)
    {
        var texView = pTexViews[(byte)view];
        if (texView is not null)
            return texView;

        var texViewPtr = GetDefaultViewPtr(Handle, view);
        texView = ObjectRegistry.Acquire(texViewPtr) as TextureViewImpl;
        if (texView is not null) 
            return texView;
        
        pTexViews[(byte)view] = texView = new TextureViewImpl(texViewPtr, Desc.Size);
        texView.AddRef();

        return texView;
    }

    public static void ValidateTextureView(TextureViewType viewType, IntPtr texView)
    {
        if (texView == IntPtr.Zero)
            throw new NullReferenceException($"There´s no default viewType for '{viewType}'.");
    }
    public static unsafe void GetObjectDesc(IntPtr ptr, out TextureDesc output)
    {
        var sizeOf = Unsafe.SizeOf<TextureDescDto>();
        var dto = new TextureDescDto();
        var descPtr = NativeApis.js_malloc(sizeOf);
        NativeApis.js_memset(descPtr, 0x0, sizeOf);
        
        js_rengine_texture_getdesc(ptr, descPtr);
        fixed(void* dataPtr = dto)
            NativeApis.js_memcpy(descPtr, dataPtr, sizeOf);
        
        var desc = new TextureDesc();
        dto.CopyTo(ref desc);
        output = desc;
        
        NativeApis.js_free(descPtr);
    }
    
    public static GPUObjectType GetObjectTypeFromDesc(in TextureDesc desc)
    {
        var dim = desc.Dimension;
        var flags = desc.BindFlags;

        var result = GPUObjectType.Unknown;
        switch (dim)
        {
            case TextureDimension.Tex1D:
                result = GPUObjectType.Texture1D;
                break;
            case TextureDimension.Tex1DArray:
                result = GPUObjectType.TextureArray;
                break;
            case TextureDimension.Tex2D:
            case TextureDimension.Tex2DArray:
                result = GPUObjectType.Texture2D;
                break;
            case TextureDimension.Tex3D:
                result = GPUObjectType.Texture3D;
                break;
            case TextureDimension.Undefined:
                break;
            case TextureDimension.Buffer:
                break;
            case TextureDimension.Tex3DArray:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if ((flags & BindFlags.RenderTarget) != 0)
            result |= GPUObjectType.RenderTarget;

        return result;
    }

    public static unsafe nint GetDefaultViewPtr(IntPtr tex, TextureViewType viewType)
    {
        var result = new ResultNative();
        var sizeOf = Unsafe.SizeOf<ResultNative>();
        var resultPtr = NativeApis.js_malloc(sizeOf);
        NativeApis.js_memset(resultPtr, 0x0, sizeOf);
        
        js_rengine_texture_getdefaultview(tex, (int)viewType, resultPtr);
        fixed(void* dataPtr = result)
            NativeApis.js_memcpy(resultPtr, dataPtr, sizeOf);
        NativeApis.js_free(resultPtr);

        if (result.Error != IntPtr.Zero)
            throw new Exception(NativeApis.js_get_string(result.Error));
        ValidateTextureView(viewType, result.Value);
        return result.Value;
    }
}

internal class InternalTexture : ITexture
{
    public IntPtr Handle { get; internal set; }
    public bool IsDisposed => false;
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.Texture;
    public string Name => Desc.Name;

    public ResourceState State
    {
        get => (ResourceState)TextureImpl.js_rengine_texture_get_state(Handle);
        set => TextureImpl.js_rengine_texture_set_state(Handle, (int)value);
    }

    public ulong GPUHandle => (ulong)TextureImpl.js_rengine_texture_get_gpuhandle(Handle).ToInt64();
    public TextureDesc Desc { get; }

    public InternalTexture(IntPtr handle)
    {
        Handle = handle;
        TextureImpl.GetObjectDesc(handle, out var desc);
        Desc = desc;
    }
    
    public void Dispose()
    {
    }
    
    public unsafe ITextureView GetDefaultView(TextureViewType view)
    {
        var texViewPtr = TextureImpl.GetDefaultViewPtr(Handle, view);
        return new InternalTextureView(texViewPtr, Desc.Size);
    }
}