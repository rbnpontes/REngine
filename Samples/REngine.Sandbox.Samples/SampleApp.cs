#if ANDROID
using App = REngine.Core.Android.App;
#else
using App = REngine.Core.Desktop.App;
#endif
using System.Drawing;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Sandbox.PongGame;

namespace REngine.Sandbox.Samples;

public class SampleApp : App
{
    private readonly SampleWindow pSampleWindow = new();
     public override Task OnStart(IServiceProvider provider)
     {
         // Ref all external libraries
         // this will make visible to Reflection
         Type[] unused =
         [
            typeof(PongGameSample)
         ];
         
         var window = MainWindow;
         window.Title = "[REngine] Samples";
         window.Size = new Size(800, 500);
         
         provider.Get<EngineEvents>().OnBeforeStop += OnBeforeStop;
         pSampleWindow.EngineStart(provider);
         return base.OnStart(provider);
     }

     private void OnBeforeStop(object? sender, EventArgs e)
     {
         pSampleWindow.EngineStop();
     }

     public override void OnUpdate(IServiceProvider provider)
     {
         pSampleWindow.EngineUpdate(provider);
     }
}