#if ANDROID
using App = REngine.Core.Android.App;
#else
using App = REngine.Core.Desktop.App;
#endif
using System.Drawing;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Threading;
using REngine.Sandbox.PongGame;

namespace REngine.Sandbox.Samples;

public class SampleApp : App
{
    private readonly SampleWindow pSampleWindow = new();
     public override async Task OnStart(IServiceProvider provider)
     {
         await base.OnStart(provider);
         var dispatcher = provider.Get<IDispatcher>();
         // Ref all external libraries
         // this will make visible to Reflection
         Type[] unused =
         [
            typeof(PongGameSample)
         ];

         // Change window properties must do on main thread
         await dispatcher.InvokeAsync(() =>
         {
             var window = MainWindow;
             window.Title = "[REngine] Samples";
             window.Size = new Size(800, 500);
         });
         
         provider.Get<EngineEvents>().OnBeforeStop.Once(OnBeforeStop);
         pSampleWindow.EngineStart(provider);
     }

     private async Task OnBeforeStop(object sender)
     {
         await EngineGlobals.MainDispatcher.Yield();
         pSampleWindow.EngineStop();
     }

     public override void OnUpdate(IServiceProvider provider)
     {
         pSampleWindow.EngineUpdate(provider);
     }
}