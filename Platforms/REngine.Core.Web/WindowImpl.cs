using System.Drawing;
using System.Numerics;
using REngine.Core.IO;
using REngine.Core.Mathematics;

namespace REngine.Core.Web;

internal partial class WindowImpl : IWindow
{
    private readonly IDisposable? pResizeEvent;
    private readonly HTMLElement pElement;
    private readonly bool pUseParentSize;
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

    // TODO: cache this call and reduce redundant JS marshal calls
    public Rectangle Bounds
    {
        get => DomUtils.GetElementBounds(pUseParentSize ? pElement.Parent : pElement).ToRect();
        set { }
    }
    // TODO: cache this call and reduce redundant JS marshal calls
    public Size Size
    {
        get => DomUtils.GetElementSize(pUseParentSize ? pElement.Parent : pElement).ToSize();
        set { }
    }
    // TODO: cache this call and reduce redundant JS marshal calls
    public Point Position
    {
        get => Point.Empty;
        set { }
    }
    public Size MinSize
    {
        get => DomUtils.GetElementMinSize(pUseParentSize ? pElement.Parent : pElement).ToSize();
        set => DomUtils.SetElementMinSize(pUseParentSize ? pElement.Parent : pElement, value.ToSizeF());
    }
    public Size MaxSize
    {
        get => DomUtils.GetElementMaxSize(pUseParentSize ? pElement.Parent : pElement).ToSize();
        set => DomUtils.SetElementMaxSize(pUseParentSize ? pElement.Parent : pElement, value.ToSizeF());
    }

    public bool Focused => false;
    public bool IsClosed => false;
    public bool IsMinimized => false;
    public bool IsFullscreen => DomUtils.IsFullScreen();

    public WindowImpl(HTMLElement element, bool useParentSize = true)
    {
        pUseParentSize = useParentSize;
        pElement = element;
        element.SetAttribute("tabindex", "1");
        var target = element;
        if (useParentSize)
        {
            target = element.Parent;
            DomUtils.SetElementSize(pElement, DomUtils.GetElementSize(element.Parent));
        }
        if (target is not null)
            pResizeEvent = DomUtils.ListenResizeEvent(target, HandleResize);
        
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
        DomUtils.RequestFullScreen(pUseParentSize ? pElement.Parent : pElement);
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
        var size = DomUtils.GetElementSize(pUseParentSize ? pElement.Parent : pElement);
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