using REngine.Core.Exceptions;

namespace REngine.Core.Web;

public sealed partial class JSObject : IJavaScriptContract
{
    private object pHandle;
    internal JSObject(object obj)
    {
        pHandle = obj;
    }
    
    public JSObject? Get(string propKey)
    {
#if WEB
        var obj = js_object_get_prop(pHandle, propKey);
        if (obj is null)
            return null;
        return new JSObject(obj);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public Action GetPropFunction(string propKey)
    {
#if WEB
        var call = js_object_get_prop_func(pHandle, propKey);
        return call;
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }
    
#if WEB
    public override string ToString() => WebMarshal.ToString(pHandle);
#endif
    public int ToInt() => WebMarshal.ToInt(pHandle);
    public byte ToByte() => (byte)ToInt();
    public short ToShort() => (short)ToInt();
    public double ToDouble() => WebMarshal.ToDouble(pHandle);
    public float ToFloat() => (float)ToDouble();
    
    public void Set(string propKey, JSObject obj)
    {
#if WEB
        pHandle = js_object_set_prop(pHandle, propKey, obj.GetJsObject());
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Set(string propKey, string value)
    {
#if WEB
        pHandle = js_object_set_prop_str(pHandle, propKey, value);
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Set(string propKey, int value)
    {
#if WEB
        pHandle = js_object_set_prop_int(pHandle, propKey, value);
#else 
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Set(string propKey, double value)
    {
#if WEB
        pHandle = js_object_set_prop_double(pHandle, propKey, value);
#else 
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public void Set(string propKey, float value)
    {
#if WEB
        pHandle = js_object_set_prop_double(pHandle, propKey, (double)value);
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