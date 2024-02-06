using System.Drawing;

namespace REngine.Core.Web;

public static partial class DomUtils
{
    public static HTMLElement? QuerySelector(string selector)
    {
        var target = js_query_selector(selector);
        if (target is null)
            return null;
        return new HTMLElement(target);
    }

    public static SizeF GetElementSize(HTMLElement? element)
    {
        if (element is null)
            return SizeF.Empty;
        var res = js_get_element_size(element.Handle);
        return new SizeF((float)res[0], (float)res[1]);
    }

    public static void SetElementSize(HTMLElement? element, SizeF size)
    {
        if (element is null)
            return;
        js_set_element_size(element.Handle, size.Width, size.Height);
    }

    public static IDisposable ListenResizeEvent(HTMLElement element, Action callback)
    {
        var call = js_on_resize_element(element.Handle, callback);
        return new DisposableDelegate(call);
    }
}