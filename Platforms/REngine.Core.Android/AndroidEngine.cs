using Android.Views.InputMethods;
using REngine.Android.Windows;
using REngine.Core.IO;
using REngine.Core.Threading;

namespace REngine.Core.Android;

public class AndroidEngine(
    IServiceProvider provider,
    EngineEvents events,
    IExecutionPipeline pipeline,
    EngineSettings settings,
    ILoggerFactory loggerFactory,
    BaseGameActivity baseGameActivity)
    : Engine(provider, events, pipeline, settings, loggerFactory)
{
    private bool pVisibleKeyboard;
    public override bool IsKeyboardVisible => pVisibleKeyboard;

    public override IEngine ShowKeyboard()
    {
        if (pVisibleKeyboard)
            return this;
        
        if (!IsMainThread)
        {
            baseGameActivity.RunOnUiThread(()=> ShowKeyboard());
            return this;
        }
        
        var manager = InputMethodManager.FromContext(baseGameActivity);
        if (manager is null || baseGameActivity.Window is null)
            return this;

        manager.ShowSoftInput(baseGameActivity.Window.DecorView, 0);
        pVisibleKeyboard = true;
        return this;
    }

    public override IEngine HideKeyboard()
    {
        if (!pVisibleKeyboard)
            return this;

        if (!IsMainThread)
        {
            baseGameActivity.RunOnUiThread(()=> HideKeyboard());
            return this;
        }
        
        var manager = InputMethodManager.FromContext(baseGameActivity);
        if (manager is null || baseGameActivity.Window is null)
            return this;
        
        manager.HideSoftInputFromWindow(baseGameActivity.Window.DecorView.WindowToken, HideSoftInputFlags.None);
        pVisibleKeyboard = false;
        return this;
    }
}