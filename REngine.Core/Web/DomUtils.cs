using System.Drawing;

namespace REngine.Core.Web;

public static partial class DomUtils
{
    public static void RequestFullScreen(HTMLElement? element)
    {
#if WEB
        if (element is null)
            return;
        js_request_fullscreen(element);
#endif
    }

    public static void ExitFullScreen()
    {
#if WEB
        js_exit_fullscreen();
#endif
    }

    public static bool IsFullScreen()
    {
#if WEB
        return js_is_fullscreen();
#else
        return false;
#endif
    }
    public static HTMLElement? QuerySelector(string selector)
    {
#if WEB
        var target = js_query_selector(selector);
        if (target is null)
            return null;
        return new HTMLElement(target);
#else
        return null;
#endif
    }

    public static SizeF GetElementSize(HTMLElement? element)
    {
#if WEB
        if (element is null)
            return SizeF.Empty;
        var res = js_get_element_size(element.GetJsObject());
        return new SizeF((float)res[0], (float)res[1]);
#else
        return SizeF.Empty;
#endif
    }

    public static RectangleF GetElementBounds(HTMLElement? element)
    {
#if WEB
        if(element is null)
            return RectangleF.Empty;
        var res = js_get_element_bounds(element.GetJsObject());
        return new RectangleF(
            (float)res[0],
            (float)res[1],
            (float)res[2],
            (float)res[3]);
#else
        return RectangleF.Empty;
#endif
    }
    
    public static void SetElementSize(HTMLElement? element, SizeF size)
    {
#if WEB
        if (element is null)
            return;
        js_set_element_size(element.GetJsObject(), size.Width, size.Height);
#endif
    }

    public static SizeF GetElementMinSize(HTMLElement? element)
    {
#if WEB
        if (element is null)
            return SizeF.Empty;
        var res = js_element_get_min_size(element.GetJsObject());
        return new SizeF((float)res[0], (float)res[1]);
#else
        return SizeF.Empty;
#endif
    }

    public static SizeF GetElementMaxSize(HTMLElement? element)
    {
#if WEB
        if(element is null)
            return SizeF.Empty;
        var res = js_element_get_max_size(element.GetJsObject());
        return new SizeF((float)res[0], (float)res[1]);
#else
        return SizeF.Empty;
#endif
    }

    public static void SetElementMinSize(HTMLElement? element, SizeF size)
    {
#if WEB
        if (element is null)
            return;
        js_element_set_min_size(element, size.Width, size.Height);
#endif
    }

    public static void SetElementMaxSize(HTMLElement? element, SizeF size)
    {
#if WEB
        if (element is null)
            return;
        js_element_set_max_size(element, size.Width, size.Height);
#endif
    }

    public static IDisposable ListenResizeEvent(HTMLElement element, Action callback)
    {
#if WEB
        var call = js_on_resize_element(element.GetJsObject(), callback);
        return new DisposableDelegate(call);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
}