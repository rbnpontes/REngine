using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using REngine.Core.Web;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

internal partial class SwapChainImpl : ISwapChain
{
    private IntPtr pHandle;
    private readonly InternalTextureView pColorBuffer;
    private readonly InternalTextureView? pDepthBuffer;
    public bool IsDisposed { get; private set; }
    public SwapChainDesc Desc { get; private set; }
    public SwapChainSize Size
    {
        get => Desc.Size;
        set => Resize(value.Width, value.Height, Transform);
    }

    public SwapChainTransform Transform
    {
        get => Desc.Transform;
        set
        {
            var size = Desc.Size;
            Resize(size.Width, size.Height, value);
        }
    }
    public ITextureView ColorBuffer => pColorBuffer;
    public ITextureView? DepthBuffer => pDepthBuffer;
    public uint BufferCount => Desc.BufferCount;
    
    public event EventHandler<SwapChainResizeEventArgs>? OnResize;
    public event EventHandler? OnPresent;
    public event EventHandler? OnDispose;

    public SwapChainImpl(IntPtr handle)
    {
        pHandle = handle;
        Desc = GetDesc(handle);

        var size = new TextureSize(Desc.Size.Width, Desc.Size.Height);
        var depthBufferPtr = js_rengine_swapchain_get_depthbuffer(handle);
        if (depthBufferPtr != IntPtr.Zero)
            pDepthBuffer = new InternalTextureView(depthBufferPtr, size);
        
        pColorBuffer = new InternalTextureView(js_rengine_swapchain_get_backbuffer(handle), size);
    }
    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        NativeObject.js_rengine_object_release(pHandle);
        pHandle = IntPtr.Zero;
        OnDispose?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateBuffers()
    {
        var size = new TextureSize(Size.Width, Size.Height);
        if (pDepthBuffer is not null)
        {
            var depthBufferPtr = js_rengine_swapchain_get_depthbuffer(pHandle);
            pDepthBuffer.Handle = depthBufferPtr;
            pDepthBuffer.Size = size;
        }

        pColorBuffer.Handle = js_rengine_swapchain_get_backbuffer(pHandle);
        pColorBuffer.Size = size;
    }
    
    public ISwapChain Present(bool vsync)
    {
        // Present is made automatically by the browser
        OnPresent?.Invoke(this,  EventArgs.Empty);
        return this;
    }
    public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
    {
        return Resize((uint)size.Width, (uint)size.Height, transform);
    }
    public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
    {
        var currSize = Size;
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);

        if (currSize.Width == width && currSize.Height == height)
            return this;
        
        js_rengine_swapchain_resize(pHandle, (int)width, (int)height, (int)transform);
        var desc = Desc;
        desc.Transform = transform;
        Desc = desc;
        
        UpdateBuffers();
        OnResize?.Invoke(this, new SwapChainResizeEventArgs(
            new SwapChainSize(width, height),
            transform
        ));
        return this;
    }
}