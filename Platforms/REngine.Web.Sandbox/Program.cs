using System.Drawing;
using System.Text;
using REngine.RHI;
using REngine.RHI.Web.Driver;

namespace REngine.Web.Sandbox;

public class Program
{
    private static void OnMessageEvent(MessageEventData eventData)
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("[REngine][Driver]:");
        log.AppendLine($"\tSeverity: {eventData.Severity}");
        log.AppendLine($"\tMessage: {eventData.Message}");
        log.AppendLine($"\tFunction: {eventData.Function}");
        log.AppendLine($"\tFile: {eventData.File}");
        log.Append($"\tLine: {eventData.Line}");
        
        Console.WriteLine(log);
    }

    public static async Task Main()
    {
        Console.WriteLine("[REngine]: Initializing Driver");
    
        var (driver, swapChain) = DriverFactory.Build(new DriverFactoryCreateInfo()
        {
            CanvasSelector = "#canvas",
            MessageEvent = OnMessageEvent,
        });

        var looper = DriverLooper.Build(() =>
        {
            driver.ImmediateCommand
                .SetRT(swapChain.ColorBuffer, swapChain.DepthBuffer)
                .ClearRT(swapChain.ColorBuffer, Color.Black)
                .ClearDepth(swapChain.DepthBuffer, ClearDepthStencil.Depth, 1.0f, 0);
        });

        Console.WriteLine("[REngine]: Finished");
    }
}