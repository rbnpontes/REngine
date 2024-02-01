using REngine.Core;

namespace REngine.RHI.Web.Driver;

internal class NullDriver : IGraphicsDriver
{
    public IGraphicsAdapter AdapterInfo => WebAdapter.Default;
    public GraphicsBackend Backend => GraphicsBackend.OpenGL;
    public string DriverName => "REngine - WebGL Driver";
    public IReadOnlyList<ICommandBuffer> Commands => [];
    public ICommandBuffer ImmediateCommand => NullCommandBuffer.Instance;
    public IDevice Device => NullDevice.Instance;
    
    public void Dispose()
    {
    }
    
    public ISwapChain CreateSwapchain(in SwapChainDesc desc, ref NativeWindow window)
    {
        throw new NotImplementedException();
    }

    public static readonly IGraphicsDriver Instance = new NullDriver();
}