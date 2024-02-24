using System.Text;

namespace REngine.Core.Web;

public static partial class WebMarshal
{
#if WEB
    public static object ToJsObject(this string value) => js_cast_to_obj(value);
    public static object ToJsObject(this int value) => js_cast_to_obj(value);
    public static object ToJsObject(this double value) => js_cast_to_obj(value);
    public static object ToJsObject(this float value) => js_cast_to_obj(value);
    public static object ToJsObject(this bool value) => js_cast_to_obj(value);
    public static object ToJsObject(this Action value) => js_cast_to_obj(value);
    public static object ToJsObject(this JSArray array) => array.GetJsObject();
    public static string ToString(object obj) => js_cast_to_string(obj);
    public static int ToInt(object obj) => js_cast_to_int(obj);
    public static double ToDouble(object obj) => js_cast_to_double(obj);
    public static float ToFloat(object obj) => js_cast_to_float(obj);
    public static bool ToBool(object obj) => js_cast_to_bool(obj);
    public static Action ToAction(object obj) => js_cast_to_action(obj);
#else
    public static object ToJsObject(this string value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this int value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this double value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this float value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this bool value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this Action value) => throw new RequiredPlatformException(PlatformType.Web);
    public static object ToJsObject(this JSArray array) => throw new RequiredPlatformException(PlatformType.Web);
    public static string ToString(object obj) => throw new RequiredPlatformException(PlatformType.Web);
    public static int ToInt(object obj) => throw new RequiredPlatformException(PlatformType.Web);
    public static double ToDouble(object obj) => throw new RequiredPlatformException(PlatformType.Web);
    public static float ToFloat(object obj) => throw new RequiredPlatformException(PlatformType.Web);
    public static bool ToBool(object obj) => throw new RequiredPlatformException(PlatformType.Web);
    public static Action ToAction(object obj) => throw new RequiredPlatformException(PlatformType.Web);
#endif
    public static object CreateJsObject(object obj)
    {
#if WEB
        obj = obj switch
        {
            string str => str.ToJsObject(),
            int @int => @int.ToJsObject(),
            double @double => @double.ToJsObject(),
            float single => single.ToJsObject(),
            bool @bool => @bool.ToJsObject(),
            Action action => action.ToJsObject(),
            StringBuilder str => str.ToString().ToJsObject(),
            IJavaScriptContract contract => contract.GetJsObject(),
            _ => obj.ToString().ToJsObject()
        };

        return obj;
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public static object? FromJsObject(object? obj)
    {
#if WEB
        if (obj is null)
            return null;
        var type = js_typeof(obj);
        if (type == "undefined")
            return null;
        if (type == "object")
            return obj;
        if (type == "boolean")
            return js_cast_to_bool(obj);
        if (type == "number")
            return js_cast_to_double(obj);
        if (type == "string")
            return js_cast_to_string(obj);
        if (type == "function")
            return js_cast_to_action(obj);
        throw new NotSupportedException($"Not supported typeof '{type}'");
#else
        throw new RequiredPlatformException(PlatformType.Web);
#endif
    }

    public static void CollectJsMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
#if WEB
        js_free_internal_memory();
#endif
    }
}