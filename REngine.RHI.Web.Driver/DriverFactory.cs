using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Web;
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
    public NativeWindow? Window = null;
    public ILoggerFactory? LoggerFactory = null;
    public Action<MessageEventData> MessageEvent = (evtData) => { };
    public SwapChainDesc SwapChainDesc = new();
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
        
        NativeApis.js_write_i32(nativeWindowPtr, selectorPtr.ToInt32());
        return (nativeWindowPtr, selectorPtr);
    }
    private static unsafe void ReadDriverData(IntPtr driverPtr, out DriverData result)
    {
        result = new DriverData();
        fixed (void* dataPtr = result)
            NativeApis.js_memcpy(driverPtr, dataPtr, Unsafe.SizeOf<DriverData>());
    }
    
    public static (IGraphicsDriver, ISwapChain) Build(DriverFactoryCreateInfo createInfo)
    {
        if (createInfo.Window is null)
            throw new NullReferenceException("Window is required");
        
        var loggerFactory = createInfo.LoggerFactory ?? new WebLoggerFactory();
        var selector = createInfo.Window.Value.CanvasSelector;
        var canvasElement = DomUtils.QuerySelector(selector);
        if (canvasElement is null)
            throw new NullReferenceException("Canvas Selector is not found or is not a valid selector");
        var autoResize = createInfo.SwapChainDesc.Size is { Width: 0, Height: 0 };

        if (autoResize)
        {
            var swapChainSize = DomUtils.GetElementSize(canvasElement);
            createInfo.SwapChainDesc.Size = new SwapChainSize(swapChainSize.ToSize());
        }
        
        var swapChainDesc = new SwapChainDescDto(createInfo.SwapChainDesc);
        var (settingsPtr, messageCallbackPtr) = CreateDriverSettingsPtr(createInfo);
        var (nativeWindowPtr, canvasSelectorPtr) = CreateNativeWindowPtr(selector);
        var swapChainDescPtr = SwapChainDescDto.CreateSwapChainPtr(ref swapChainDesc);
        
        var result = new DriverResult();
        js_rengine_create_driver(settingsPtr, swapChainDescPtr, nativeWindowPtr, result.Handle);
        result.Load();
        
        if (!result.Error.Equals(string.Empty))
        {
            DisposeResources();
            throw new DriverException(result.Error);
        }

        ReadDriverData(result.Driver, out var driverResult);
        DisposeResources();
        

        var commandBuffer = new CommandBufferImpl(driverResult.Context);
        var device = new DeviceImpl(driverResult.Device, loggerFactory.Build<IDevice>());
        var driver = new DriverImpl(commandBuffer, device, driverResult.Factory, messageCallbackPtr);
        
        return (driver, new SwapChainImpl(result.SwapChain));
        void DisposeResources()
        {
            result.Dispose();
            NativeApis.js_free(settingsPtr);
            NativeApis.js_free(nativeWindowPtr);
            NativeApis.js_free(canvasSelectorPtr);
            NativeApis.js_free(swapChainDescPtr);
            NativeApis.js_free(result.Driver);
        }
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