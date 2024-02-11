namespace REngine.Core.Web;
using _Guid = Guid;
public sealed partial class HTMLElement : IJavaScriptContract
{
#if WEB
    private object pHandle;
    public string Guid { get; }
#else
    public string Guid => throw new RequiredPlatformException(PlatformType.Web);
#endif

    public HTMLElement? Parent
    {
        get
        {
#if WEB
            var parent = js_get_element_parent(pHandle);
            return parent is null ? null : new HTMLElement(parent);
#else
            throw new RequiredPlatformException(PlatformType.Web);
#endif
        }
    }

#if WEB
    public string[] Attributes => js_get_element_attrs(pHandle);
#else
    public string[] Attributes => throw new RequiredPlatformException(PlatformType.Web);
#endif

#if WEB
    public string Selector => $"[data-guid=\"{Guid}\"]";
#else
    public string Selector => throw new RequiredPlatformException(PlatformType.Web);
#endif
    
    internal HTMLElement(object element)
    {
#if WEB
        pHandle = element;
        var guid = GetAttribute("data-guid");
        if(string.IsNullOrEmpty(guid))
            SetAttribute("data-guid", guid = _Guid.NewGuid().ToString());
        Guid = guid;
#endif
    }

    public string GetAttribute(string key)
    {
#if WEB
        return js_get_element_attr(pHandle, key);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void SetAttribute(string key, string value)
    {
#if WEB
        js_set_element_attr(pHandle, key, value);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Focus()
    {
#if WEB
        js_element_focus(pHandle);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
    
    public IDisposable AddEventListener(string eventName, Action<JSObject> callback)
    {
#if WEB
        var call = js_element_add_event_listener(pHandle, eventName, e =>
        {
            callback(new JSObject(e));
        });
        return new DisposableDelegate(call);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

#if WEB
    public object GetJsObject() => pHandle;
#else
    public object GetJsObject() => throw new RequiredPlatformException(PlatformType.Web);
#endif
}