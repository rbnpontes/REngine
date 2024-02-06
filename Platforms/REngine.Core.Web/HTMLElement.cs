namespace REngine.Core.Web;

public sealed partial class HTMLElement
{
    public object Handle { get; }

    public HTMLElement? Parent
    {
        get
        {
            var parent = js_get_element_parent(Handle);
            return parent is null ? null : new HTMLElement(parent);
        }
    }
    internal HTMLElement(object element)
    {
        Handle = element;
    }
}