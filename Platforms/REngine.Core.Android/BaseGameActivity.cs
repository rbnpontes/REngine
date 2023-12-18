using System.Numerics;
using System.Text;
using Android.Util;
using Android.Views;
using Java.Util.Logging;
using REngine.Android.Windows;

namespace REngine.Core.Android;

public abstract class BaseGameActivity : Activity
{
    private readonly object pSync = new();
    private GameLogic? pGameLogic;
    private GameView? pGameView;
    private CancellationTokenSource pCancellationTokenSource = new();
    
    public abstract IEngineApplication OnGetEngineApplication();

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        lock (pSync)
        {
            pCancellationTokenSource.Cancel();
            
            pGameLogic = new GameLogic(this, Assets);
            
            LinearLayout.LayoutParams layoutParams = new(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);
            
            REngine.Android.Windows.GameView gameView = new(this);
            gameView.LayoutParameters = layoutParams; 
            gameView.SetCallback(pGameLogic);
            pGameView = gameView;
            
            LinearLayout linearLayout = new (this);
            linearLayout.LayoutParameters = layoutParams;
            linearLayout.AddView(gameView);
            
            SetContentView(linearLayout);

            pCancellationTokenSource = new CancellationTokenSource();
        }
    }

    protected override void OnStop()
    {
        base.OnStop();
        pGameLogic?.Stop();
        pGameView = null;
        Log.Info(nameof(REngine), "Stop Activity");
    }
    public virtual void OnEngineError(Exception error){}

    public float GetDpi()
    {
        return Resources?.DisplayMetrics?.Density ?? 1.0f;
    }

    public Vector2 GetVideoScale()
    {
        var dpi = GetDpi();
        return new Vector2(
            (Resources?.DisplayMetrics?.WidthPixels ?? 1.0f) / dpi,
            (Resources?.DisplayMetrics?.HeightPixels ?? 1.0f) / dpi
        );
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        Log.Info(nameof(REngine), "KeyDown: " + keyCode);
        return pGameView?.OnKeyDown(keyCode, e) ?? base.OnKeyDown(keyCode, e);
    }

    public override bool OnKeyUp(Keycode keyCode, KeyEvent? e)
    {
        Log.Info(nameof(REngine), "KeyUp: " + keyCode);
        return pGameView?.OnKeyUp(keyCode, e) ?? base.OnKeyUp(keyCode, e);
    }

    public virtual void OnEngineStop()
    {
        Task.Run(() => HandleAppExit(pCancellationTokenSource.Token));
    }

    private async void HandleAppExit(CancellationToken cancellationToken)
    {
        lock (pSync)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
        }
        await Task.Delay(100);

        lock (pSync)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            pGameView = null;
            pGameLogic = null;
        }

        Log.Info(nameof(REngine), "Exiting App");
        Finish();
        Java.Lang.JavaSystem.Exit(0);
    }
}