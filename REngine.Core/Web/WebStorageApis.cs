using System.Runtime.InteropServices.JavaScript;

namespace REngine.Core.Web;

internal partial class SessionStorageImpl
{
#if WEB
    [JSImport("session_storage_length", WebLibConstants.LibName)]
    private static partial int js_session_storage_length();

    [JSImport("session_storage_keys", WebLibConstants.LibName)]
    private static partial string[] js_session_storage_keys();
    
    [JSImport("session_storage_set", WebLibConstants.LibName)]
    private static partial void js_session_storage_set(string key, string value);
    
    [JSImport("session_storage_get", WebLibConstants.LibName)]
    private static partial string js_session_storage_get(string key);
    
    [JSImport("session_storage_remove", WebLibConstants.LibName)]
    private static partial void js_session_storage_remove(string key);
    
    [JSImport("session_storage_clear", WebLibConstants.LibName)]
    private static partial void js_session_storage_clear();
    [JSImport("session_storage_contains", WebLibConstants.LibName)]
    private static partial bool js_session_storage_contains(string key);
#endif
}

internal partial class LocalStorageImpl
{
#if WEB
    [JSImport("local_storage_length", WebLibConstants.LibName)]
    private static partial int js_local_storage_length();

    [JSImport("local_storage_keys", WebLibConstants.LibName)]
    private static partial string[] js_local_storage_keys();

    [JSImport("local_storage_set", WebLibConstants.LibName)]
    private static partial void js_local_storage_set(string key, string value);

    [JSImport("local_storage_get", WebLibConstants.LibName)]
    private static partial string js_local_storage_get(string key);

    [JSImport("local_storage_remove", WebLibConstants.LibName)]
    private static partial void js_local_storage_remove(string key);

    [JSImport("local_storage_clear", WebLibConstants.LibName)]
    private static partial void js_local_storage_clear();
    [JSImport("local_storage_contains", WebLibConstants.LibName)]
    private static partial bool js_local_storage_contains(string key);
#endif
}