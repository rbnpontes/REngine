namespace REngine.Core.Web;

public static partial class WebFetch
{
    private static async Task<byte[]> DoFetch(string url, string method)
    {
        var result = new JSObject(await js_fetch(url, method));
        var buffer = new byte[result.Get("length")?.ToInt() ?? 0];
        js_fetch_read_result(result.GetJsObject(), buffer.AsSpan());
        return buffer;
    }
    public static async Task<byte[]> Get(string url)
    {
        return await DoFetch(url, "GET");
    }

    public static async Task<byte[]> Post(string url)
    {
        return await DoFetch(url, "POST");
    }
}