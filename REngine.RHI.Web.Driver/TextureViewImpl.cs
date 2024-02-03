using System.Runtime.CompilerServices;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class TextureViewImpl : NativeObject, ITextureView
{
    public GPUObjectType ObjectType => GPUObjectType.TextureView;
    public string Name { get; }
    public ITexture Parent { get; }
    public TextureViewDesc Desc { get; }
    public TextureViewType ViewType => Desc.ViewType;
    public TextureSize Size { get; }
    
    public TextureViewImpl(IntPtr handle, TextureSize size) : base(handle)
    {
        Size = size;
        GetObjectDesc(handle, out var desc);
        Desc = desc;
        Name = GetObjectName(handle);
        Parent = NullTexture.Instance;
    }

    public static unsafe void GetObjectDesc(IntPtr ptr, out TextureViewDesc desc)
    {
        var sizeOf = Unsafe.SizeOf<TextureViewDescDto>();
        var dto = new TextureViewDescDto();
        var descPtr = NativeApis.js_malloc(sizeOf);
        
        js_rengine_textureview_getdesc(ptr, descPtr);
        fixed(void* dataPtr = dto)
            NativeApis.js_memcpy(descPtr, dataPtr, sizeOf);
        NativeApis.js_free(descPtr);

        var output = new TextureViewDesc();
        dto.CopyTo(ref output);
        desc = output;
    }
}

internal class InternalTextureView : ITextureView
{
    public IntPtr Handle { get; internal set; }
    public bool IsDisposed => false;
    
    public event EventHandler? OnDispose;
    public GPUObjectType ObjectType => GPUObjectType.TextureView;
    public string Name { get; }
    public ITexture Parent { get; }
    public TextureViewDesc Desc { get; }
    public TextureViewType ViewType => Desc.ViewType;
    public TextureSize Size { get; internal set; }

    public InternalTextureView(IntPtr handle, TextureSize size)
    {
        Handle = handle;
        Size = size;
        Name = NativeObject.GetObjectName(handle);
        TextureViewImpl.GetObjectDesc(handle, out var desc);
        Desc = desc;
        Parent = new InternalTexture(TextureViewImpl.GetTextureParentPtr(Handle));
    }
    
    public void Dispose()
    {
    }
}