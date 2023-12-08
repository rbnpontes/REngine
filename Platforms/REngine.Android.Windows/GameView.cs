using System.Drawing;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Size = System.Drawing.Size;

namespace REngine.Android.Windows;

public sealed class GameView : SurfaceView, ISurfaceHolderCallback
{
    private IGameViewCallback? pCallback;
 
    public IntPtr NativeWindow { get; private set; } = IntPtr.Zero;
    public Rectangle Bounds => new Rectangle(Left, Top, Width, Height);
    public Size Size => new Size(Width, Height);
    
    public GameView(Context? context) : base(context)
    {
    }

    public void SetCallback(IGameViewCallback callback)
    {
        pCallback = callback;
    }
    
    private void UpdateNativeWindowPtr(ISurfaceHolder holder)
    {
        var surface = holder.Surface;
        if (surface is null)
            return;
        NativeWindow = AndroidApis.ANativeWindow_fromSurface(JNIEnv.Handle, surface.Handle);
    }
    
    public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
    {
        UpdateNativeWindowPtr(holder);
        pCallback?.OnGameViewChange(this);
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        UpdateNativeWindowPtr(holder);
        pCallback?.OnGameViewReady(this);
    }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        UpdateNativeWindowPtr(holder);
        pCallback?.OnGameViewDestroy(this);
    }
}