namespace REngine.Core.Web;

public static partial class WebFrame
{
    public static void Alert(string message)
    {
#if WEB
        js_alert(message);
#endif
    }
}