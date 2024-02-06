using REngine.Core.Web.IO;

namespace REngine.Core.Web;

public interface IWebStorage
{
    public int Length { get; }
    public string[] Keys { get; }
    public string GetItem(string key);
    
    public IWebStorage SetItem(string key, string value);
    public IWebStorage RemoveItem(string key);
    public IWebStorage Clear();

    public bool Contains(string key);
}
public static class WebStorage
{
     public static IWebStorage GetSessionStorage() => SessionStorageImpl.Instance;
     public static IWebStorage GetLocalStorage() => LocalStorageImpl.Instance;
}