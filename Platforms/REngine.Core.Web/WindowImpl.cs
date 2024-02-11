using System.Drawing;
using System.Numerics;
using REngine.Core.IO;
using REngine.Core.Mathematics;

namespace REngine.Core.Web;

internal partial class WindowImpl : IWindow
{
    private readonly IDisposable? pResizeEvent;
    private readonly HTMLElement pElement;
    public event WindowEvent? OnUpdate;
    public event WindowEvent? OnShow;
    public event WindowEvent? OnClose;
    public event WindowInputEvent? OnKeyDown;
    public event WindowInputEvent? OnKeyUp;
    public event WindowInputTextEvent? OnInput;
    public event WindowResizeEvent? OnResize;
    public event WindowMouseEvent? OnMouseDown;
    public event WindowMouseEvent? OnMouseUp;
    public event WindowMouseEvent? OnMouseMove;
    public event WindowMouseWheelEvent? OnMouseWheel;

    private readonly List<IDisposable> pDisposables;
    private bool pDisposed;

    public string Title { get; set; } = string.Empty;
    public IntPtr Handle => IntPtr.Zero;

    public Rectangle Bounds
    {
        get => DomUtils.GetElementBounds(pElement).ToRect();
        set { }
    }

    public Size Size
    {
        get => Bounds.Size;
        set { }
    }

    public Point Position
    {
        get => Bounds.Location;
        set { }
    }

    public Size MinSize
    {
        get => DomUtils.GetElementMinSize(pElement).ToSize();
        set => DomUtils.SetElementMinSize(pElement, value.ToSizeF());
    }

    public Size MaxSize
    {
        get => DomUtils.GetElementMaxSize(pElement).ToSize();
        set => DomUtils.SetElementMaxSize(pElement, value.ToSizeF());
    }

    public bool Focused => false;
    public bool IsClosed => false;
    public bool IsMinimized => false;
    public bool IsFullscreen => DomUtils.IsFullScreen();

    public WindowImpl(HTMLElement element)
    {
        pElement = element;
        element.SetAttribute("tabindex", "1");
        var parent = element.Parent;
        if (parent is not null)
            pResizeEvent = DomUtils.ListenResizeEvent(parent, HandleResize);
        
        pDisposables =
        [
            element.AddEventListener("keydown", HandleKeyDown),
            element.AddEventListener("keyup", HandleKeyUp),
            element.AddEventListener("keypress", HandleKeyPress),
            element.AddEventListener("mousedown", HandleMouseDown),
            element.AddEventListener("mouseup", HandleMouseUp),
            element.AddEventListener("mousemove", HandleMouseMove),
            element.AddEventListener("wheel", HandleMouseWheel),
            element.AddEventListener("contextmenu", HandleMenuContext)
        ];
    }
    
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        pResizeEvent?.Dispose();
        pDisposables.ForEach(x => x.Dispose());
    }

    public IWindow Close()
    {
        return this;
    }

    public IWindow Show()
    {
        return this;
    }

    public IWindow Focus()
    {
        return this;
    }

    public IWindow Update()
    {
        return this;
    }

    public IWindow Fullscreen()
    {
        DomUtils.RequestFullScreen(pElement);
        return this;
    }

    public IWindow ExitFullscreen()
    {
        DomUtils.ExitFullScreen();
        return this;
    }

    public IWindow GetNativeWindow(out NativeWindow window)
    {
        window = new NativeWindow() { CanvasSelector = pElement.Selector };
        return this;
    }

    private void HandleResize()
    {
        var size = DomUtils.GetElementSize(pElement.Parent);
        DomUtils.SetElementSize(pElement, size);
        
        OnResize?.Invoke(this, new WindowResizeEventArgs(size.ToSize(), pElement, IntPtr.Zero));
    }

    private void HandleKeyDown(JSObject evt)
    {
        var key = evt.Get("key")?.ToString() ?? string.Empty;
        if(key.Length > 1)
            evt.GetPropFunction("preventDefault")();
        var keyCode = evt.Get("keyCode")?.ToInt() ?? 0;
        OnKeyDown?.Invoke(this, 
            new WindowInputEventArgs(InputConverter.GetInputKey(keyCode), pElement, IntPtr.Zero)
        );
    }

    private void HandleKeyUp(JSObject evt)
    {
        evt.GetPropFunction("preventDefault")();
        var keyCode = evt.Get("keyCode")?.ToInt() ?? 0;
        OnKeyUp?.Invoke(this,
            new WindowInputEventArgs(InputConverter.GetInputKey(keyCode), pElement, IntPtr.Zero)
        );
    }

    private void HandleKeyPress(JSObject evt)
    {
        var charCode = evt.Get("charCode")?.ToInt() ?? 0;
        if (charCode == 0)
            return;
        var input = char.ConvertFromUtf32(charCode);
        OnInput?.Invoke(this, new WindowInputTextEventArgs(input, pElement, IntPtr.Zero));
    }
    
    private void HandleMouseDown(JSObject evt)
    {
        pElement.Focus();
        HandleMouseButtonAction(evt, true);
    }

    private void HandleMouseUp(JSObject evt)
    {
        HandleMouseButtonAction(evt, false);
    }

    private void HandleMenuContext(JSObject evt)
    {
        evt.GetPropFunction("preventDefault")();
    }

    private void HandleMouseButtonAction(JSObject evt, bool isDown)
    {
        evt.GetPropFunction("preventDefault")();
        var button = evt.Get("button")?.ToInt() ?? 0;
        var key = InputConverter.GetMouseKey(button);
        (isDown ? OnMouseDown : OnMouseUp)?.Invoke(this, 
            new WindowMouseEventArgs(key, pElement, IntPtr.Zero));
    }
    
    private void HandleMouseMove(JSObject evt)
    {
        var posX = evt.Get("clientX")?.ToFloat() ?? -1;
        var posY = evt.Get("clientY")?.ToFloat() ?? -1;
        
        OnMouseMove?.Invoke(this, new WindowMouseEventArgs(MouseKey.None, pElement, IntPtr.Zero)
        {
            Position = new Vector2(posX, posY)
        });
    }

    private void HandleMouseWheel(JSObject evt)
    {
        evt.GetPropFunction("preventDefault")();
        var deltaX = evt.Get("deltaX")?.ToFloat() ?? 0;
        var deltaY = evt.Get("deltaY")?.ToFloat() ?? 0;
        
        OnMouseWheel?.Invoke(this, new WindowMouseWheelEventArgs(
            new Vector2(deltaX, deltaY), pElement, IntPtr.Zero)
        );
    }

    public IWindow ForwardKeyDownEvent(InputKey key)
    {
        throw new NotSupportedException();
    }

    public IWindow ForwardKeyUpEvent(InputKey key)
    {
        throw new NotSupportedException();
    }

    public IWindow ForwardInputEvent(int utf32Char)
    {
        throw new NotSupportedException();
    }
}