namespace REngine.Core.Web;

public sealed class HTMLElement
{
    public object Handle { get; }
    internal HTMLElement(object element)
    {
        Handle = element;
    }
}