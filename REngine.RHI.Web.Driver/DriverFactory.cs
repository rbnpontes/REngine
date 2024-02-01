using System.Runtime.CompilerServices;
using System.Text;
using REngine.RHI.Web.Driver.Models;

namespace REngine.RHI.Web.Driver;

public enum DbgMsgSeverity
{
    Info =0,
    Warning,
    Error,
    FatalError
}
public struct MessageEventData()
{
    public DbgMsgSeverity Severity;
    public string Message = string.Empty;
    public string Function = string.Empty;
    public string File = string.Empty;
    public int Line;
}
public struct DriverFactoryCreateInfo()
{
    public string CanvasSelector = string.Empty;
    public Action<MessageEventData> MessageEvent = (evtData) => { };
    public SwapChainDesc SwapChainDesc = new SwapChainDesc();
}

public static partial class DriverFactory
{
    private static unsafe (IntPtr, IntPtr) CreateDriverSettingsPtr(DriverFactoryCreateInfo createInfo)
    {
        var messageCallbackPtr =
            NativeApis.js_register_function(()=> HandleDriverMessage(createInfo.MessageEvent), "viiiii");
        var driverSettingsPtr = NativeApis.js_malloc(Unsafe.SizeOf<GraphicsDriverSettingsDto>());
        var driverSettings = new GraphicsDriverSettingsDto() { MessageCallback = messageCallbackPtr };

        fixed (void* dataPtr = driverSettings)
            NativeApis.js_memcpy(dataPtr, driverSettingsPtr, Unsafe.SizeOf<GraphicsDriverSettingsDto>());
        
        return (driverSettingsPtr, messageCallbackPtr);
    }
    private static (IntPtr, IntPtr) CreateNativeWindowPtr(string canvasSelector)
    {
        var nativeWindowPtr = NativeApis.js_malloc(NativeApis.js_get_ptr_size());
        var selectorPtr = NativeApis.js_alloc_string(canvasSelector);
        
        NativeApis.js_writei32(nativeWindowPtr, selectorPtr.ToInt32());
        return (nativeWindowPtr, selectorPtr);
    }
    
    public static (IGraphicsDriver, ISwapChain) Build(DriverFactoryCreateInfo createInfo)
    {
        var canvasElement = NativeApis.js_query_selector(createInfo.CanvasSelector);
        if (canvasElement is null)
            throw new NullReferenceException("Canvas Selector is not found or is not a valid selector");
        if (createInfo.SwapChainDesc.Size.Width == 0 && createInfo.SwapChainDesc.Size.Height == 0)
        {
            var swapChainSize = NativeApis.js_get_element_size(canvasElement);
            createInfo.SwapChainDesc.Size = new SwapChainSize((uint)swapChainSize[0], (uint)swapChainSize[1]);
        }
        
        var swapChainDesc = new SwapChainDescDto(createInfo.SwapChainDesc);
        
        var (settingsPtr, messageCallbackPtr) = CreateDriverSettingsPtr(createInfo);
        var (nativeWindowPtr, canvasSelectorPtr) = CreateNativeWindowPtr(createInfo.CanvasSelector);
        var swapChainDescPtr = SwapChainDescDto.CreateSwapChainPtr(ref swapChainDesc);
        
        var result = new DriverResult();
        Console.WriteLine("Result Ptr: "+result.Handle.ToInt32());
        js_rengine_create_driver(settingsPtr, swapChainDescPtr, nativeWindowPtr, result.Handle);
        Console.WriteLine("Loading Ptr");
        result.Load();
        result.Dispose();

        NativeApis.js_free(settingsPtr);
        NativeApis.js_free(nativeWindowPtr);
        NativeApis.js_free(swapChainDescPtr);
        
        if (!result.Error.Equals(string.Empty))
            throw new DriverException(result.Error);
        
        Console.WriteLine($"Driver: {result.Driver.ToInt32()}");
        Console.WriteLine($"SwapChain: {result.SwapChain.ToInt32()}");
        return (NullDriver.Instance, NullSwapChain.Instance);
    }

    private static void HandleDriverMessage(Action<MessageEventData> callback)
    {
        var eventData = NativeApis.js_get_last_method_v0();
        if (eventData.Length == 0)
            return;
        callback(new MessageEventData()
        {
            Severity = (DbgMsgSeverity)eventData[0],
            Message = NativeApis.js_get_string(new IntPtr(eventData[1])),
            Function = NativeApis.js_get_string(new IntPtr(eventData[2])),
            File = NativeApis.js_get_string(new IntPtr(eventData[3])),
            Line = eventData[4]
        });
    }
}