using System.Text;
using Android.Content.Res;
using Android.Util;
using REngine.Android.Windows;

namespace REngine.Core.Android;

internal class GameLogic(
    BaseGameActivity activity,
    AssetManager assetManager) : IGameViewCallback
{
    private Task? pGameTask;
    private AndroidEngineInstance? pEngineStartup;
    
    public void Stop()
    {
        pEngineStartup?.Stop();
        pGameTask?.Wait(0);
    }
    
    private void StartEngine(REngine.Android.Windows.GameView gameView)
    {
        pEngineStartup = new AndroidEngineInstance(
            activity,
            gameView,
            assetManager,
            activity.OnGetEngineApplication()
        );

        try
        {
            pEngineStartup
                .Setup()
                .Start()
                .Run();
        }
        catch (Exception e)
        {
            var err = new StringBuilder();
            err.Append($"Error Type: {e.GetType().Name}");
            err.AppendLine($"Message: {e.Message}");
            err.AppendLine($"StackTrace: {e.StackTrace}");
				
            var depthCount = 1;
            var innerException = e.InnerException;
            while (innerException != null)
            {
                var spacemenChars = new char[depthCount];
                for (var i = 0; i < depthCount; ++i)
                    spacemenChars[i] = '\t';
                err.Append(spacemenChars);
                err.AppendLine("------- Inner Exception -------");
                err.Append(spacemenChars);
                err.AppendLine($"Error Type: {innerException.GetType().Name}");
                err.Append(spacemenChars);
                err.AppendLine($"Message: {innerException.Message}");
                err.Append(spacemenChars);
                err.AppendLine("StackTrace:");

                var stackTrace = (innerException.StackTrace ?? string.Empty).Split('\n');
                foreach (var stackTraceLine in stackTrace)
                {
                    err.Append(spacemenChars);
                    err.AppendLine(stackTraceLine);
                }

                innerException = innerException.InnerException;
                ++depthCount;
            }

            Log.Error(GetType().Name, err.ToString());
            activity.OnEngineError(e);
            activity.RunOnUiThread(()=> throw e);
        }
    }

    public void OnGameViewChange(REngine.Android.Windows.GameView view)
    {
        Log.Info(GetType().Name, "GameView Changed");
    }

    public void OnGameViewReady(REngine.Android.Windows.GameView view)
    {
        pGameTask = Task.Factory.StartNew(() =>
        {
            StartEngine(view);
        }, TaskCreationOptions.LongRunning);
    }

    public void OnGameViewDestroy(REngine.Android.Windows.GameView view)
    {
        Log.Info(GetType().Name, "GameView Destroyed");
    }
}