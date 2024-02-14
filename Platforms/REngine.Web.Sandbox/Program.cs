using System.Drawing;
using System.Text;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Serialization;
using REngine.Core.Web;
using REngine.RHI;
using REngine.RHI.Web.Driver;
using REngine.Sandbox.Samples;

namespace REngine.Web.Sandbox;

public class Program
{
    public static async Task Main()
    {
        var engineInstance =WebEngineInstance.CreateStartup<SampleApp>();
        await engineInstance.Setup();
        await engineInstance.Start();
        await engineInstance.Run();
    }
}