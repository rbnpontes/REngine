using REngine.Core;

namespace REngine.RHI.Web.Driver;

internal class DriverImpl(
    ICommandBuffer commandBuffer,
    IDevice device,
    IntPtr factory,
    IntPtr msgCallPtr) : IGraphicsDriver
{
    private bool pDisposed;
    public IGraphicsAdapter AdapterInfo => WebAdapter.Default;
    public GraphicsBackend Backend => GraphicsBackend.OpenGL;
    public string DriverName => "REngine Web Driver";
    public IReadOnlyList<ICommandBuffer> Commands => [];
    public ICommandBuffer ImmediateCommand => commandBuffer;
    public IDevice Device => device;
    
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        ImmediateCommand.Dispose();
        Device.Dispose();
        
        NativeApis.js_unregister_function(msgCallPtr);
    }
    
    public ISwapChain CreateSwapchain(in SwapChainDesc desc, ref NativeWindow window)
    {
        throw new NotSupportedException("Web driver does not support SwapChain creation");
    }
}