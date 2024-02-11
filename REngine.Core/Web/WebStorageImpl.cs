using REngine.Core.Storage;

namespace REngine.Core.Web;

#if WEB
internal partial class SessionStorageImpl : IWebStorage
{
    public static readonly SessionStorageImpl Instance = new();
    public int Length => js_session_storage_length();
    public string[] Keys => js_session_storage_keys();
    public string GetItem(string key) => js_session_storage_get(key);

    public IWebStorage SetItem(string key, string value)
    {
        js_session_storage_set(key, value);
        return this;
    }

    public IWebStorage RemoveItem(string key)
    {
        js_session_storage_remove(key);
        return this;
    }

    public IWebStorage Clear()
    {
        js_session_storage_clear();
        return this;
    }

    public bool Contains(string key) => js_session_storage_contains(key);
}

internal partial class LocalStorageImpl : IWebStorage
{
    public static readonly LocalStorageImpl Instance = new();
    public int Length => js_local_storage_length();
    public string[] Keys => js_local_storage_keys();
    public string GetItem(string key) => js_local_storage_get(key);

    public IWebStorage SetItem(string key, string value)
    {
        js_local_storage_set(key, value);
        return this;
    }

    public IWebStorage RemoveItem(string key)
    {
        js_local_storage_remove(key);
        return this;
    }

    public IWebStorage Clear()
    {
        js_local_storage_clear();
        return this;
    }
    
    public bool Contains(string key) => js_local_storage_contains(key);
}
#endif

internal class GlobalStorageImpl : IWebStorage
{
    public int Length => GlobalStorage.Keys.Length;
    public string[] Keys => GlobalStorage.Keys;
    public string GetItem(string key)
    {
        return GlobalStorage.GetItem<string>(key) ?? string.Empty;
    }

    public IWebStorage SetItem(string key, string value)
    {
        GlobalStorage.SetItem(key, value);
        return this;
    }

    public IWebStorage RemoveItem(string key)
    {
        GlobalStorage.RemoveItem(key);
        return this;
    }

    public IWebStorage Clear()
    {
        GlobalStorage.ClearItems();
        return this;
    }

    public bool Contains(string key)
    {
        return GlobalStorage.Contains(key);
    }

    public static readonly GlobalStorageImpl Instance = new();
}