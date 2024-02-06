using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web.IO;

internal partial class SessionStorageImpl
{
    [JSImport("session_storage_length", Constants.LibName)]
    private static partial int js_session_storage_length();

    [JSImport("session_storage_keys", Constants.LibName)]
    private static partial string[] js_session_storage_keys();
    
    [JSImport("session_storage_set", Constants.LibName)]
    private static partial void js_session_storage_set(string key, string value);
    
    [JSImport("session_storage_get", Constants.LibName)]
    private static partial string js_session_storage_get(string key);
    
    [JSImport("session_storage_remove", Constants.LibName)]
    private static partial void js_session_storage_remove(string key);
    
    [JSImport("session_storage_clear", Constants.LibName)]
    private static partial void js_session_storage_clear();
    [JSImport("session_storage_contains", Constants.LibName)]
    private static partial bool js_session_storage_contains(string key);
}

internal partial class LocalStorageImpl
{
    [JSImport("local_storage_length", Constants.LibName)]
    private static partial int js_local_storage_length();

    [JSImport("local_storage_keys", Constants.LibName)]
    private static partial string[] js_local_storage_keys();

    [JSImport("local_storage_set", Constants.LibName)]
    private static partial void js_local_storage_set(string key, string value);

    [JSImport("local_storage_get", Constants.LibName)]
    private static partial string js_local_storage_get(string key);

    [JSImport("local_storage_remove", Constants.LibName)]
    private static partial void js_local_storage_remove(string key);

    [JSImport("local_storage_clear", Constants.LibName)]
    private static partial void js_local_storage_clear();
    [JSImport("local_storage_contains", Constants.LibName)]
    private static partial bool js_local_storage_contains(string key);
}