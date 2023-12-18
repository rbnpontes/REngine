using System.Drawing;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Size = System.Drawing.Size;

namespace REngine.Android.Windows;

public sealed class GameView : SurfaceView, ISurfaceHolderCallback
{
    private IGameViewCallback? pCallback;
    private IGameViewKeyboardListener? pKeyboardListener;
 
    public IntPtr NativeWindow { get; private set; } = IntPtr.Zero;
    public Rectangle Bounds => new Rectangle(Left, Top, Width, Height);
    public Size Size => new Size(Width, Height);
    
    public GameView(Context? context) : base(context)
    {
        Holder?.AddCallback(this);
        Focusable = true;
    }

    public void SetCallback(IGameViewCallback? callback)
    {
        pCallback = callback;
    }

    public void SetKeyboardListener(IGameViewKeyboardListener? callback)
    {
        pKeyboardListener = callback;
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

    public override IInputConnection? OnCreateInputConnection(EditorInfo? outAttrs)
    {
        if (outAttrs != null)
            outAttrs.ImeOptions = ImeFlags.NoFullscreen;
        return base.OnCreateInputConnection(outAttrs);
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (e is null || pKeyboardListener is null)
            return base.OnKeyDown(keyCode, e);
        
        var handled = pKeyboardListener.OnGameViewKeyDown(keyCode, e.UnicodeChar);
        return handled || base.OnKeyDown(keyCode, e);
    }

    public override bool OnKeyUp(Keycode keyCode, KeyEvent? e)
    {
        if(pKeyboardListener is null)
            return base.OnKeyUp(keyCode, e);

        var handled = pKeyboardListener.OnGameViewKeyUp(keyCode);
        return handled || base.OnKeyUp(keyCode, e);
    }
}