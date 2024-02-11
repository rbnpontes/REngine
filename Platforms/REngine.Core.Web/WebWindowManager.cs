using System.Numerics;
using REngine.Core.Web;

namespace REngine.Core.Desktop;

public sealed class WebWindowManager : IWindowManager
{
    private readonly List<IWindow> pWindows = [];
    private bool pDisposed;
    
    public IReadOnlyList<IWindow> Windows => pWindows;
    public Vector2 VideoScale => Vector2.One;

    public IWindowManager CloseAllWindows()
    {
        return this;
    }

    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
    }
    public IWindow Create(WindowCreationInfo createInfo)
    {
        if(createInfo.WindowInstance is null)
            throw new NotSupportedException("Window Instance is required. Any other operations is not allowed.");
        if (createInfo.WindowInstance is string selector)
            return Create(selector);
        if (createInfo.WindowInstance is HTMLElement element)
            return Create(element);
        throw new NotSupportedException($"Not supported window instance type of '{createInfo.WindowInstance.GetType().Name}'");
    }

    public IWindow Create(string canvasSelector)
    {
        var element = DomUtils.QuerySelector(canvasSelector);
        if (element is null)
            throw new NullReferenceException("Could not possible to found canvas by selector: " + canvasSelector);
        return Create(element);
    }

    public IWindow Create(HTMLElement element)
    {
        var window = new WindowImpl(element);
        pWindows.Add(window);
        return window;
    }
}