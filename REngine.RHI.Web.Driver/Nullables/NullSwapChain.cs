using System.Drawing;

namespace REngine.RHI.Web.Driver;

internal class NullSwapChain : ISwapChain
{
    public event EventHandler<SwapChainResizeEventArgs>? OnResize;
    public event EventHandler? OnPresent;
    public event EventHandler? OnDispose;
    public SwapChainDesc Desc { get; } = new();
    public SwapChainSize Size { get; set; } = new();
    public SwapChainTransform Transform { get; set; } = SwapChainTransform.Identity;
    public ITextureView ColorBuffer { get; }
    public ITextureView? DepthBuffer { get; }
    public uint BufferCount { get; } = 0;
    
    public void Dispose()
    {
    }
    public ISwapChain Present(bool vsync)
    {
        return this;
    }
    public ISwapChain Resize(Size size, SwapChainTransform transform = SwapChainTransform.Optimal)
    {
        return this;
    }
    public ISwapChain Resize(uint width, uint height, SwapChainTransform transform = SwapChainTransform.Optimal)
    {
        return this;
    }

    public static NullSwapChain Instance = new();
}