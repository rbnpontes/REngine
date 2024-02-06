using System.Text;

namespace REngine.Core.Web;

public static partial class WebMarshal
{
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

    public static object CreateJsObject(object obj)
    {
        obj = obj switch
        {
            string str => str.ToJsObject(),
            int @int => @int.ToJsObject(),
            double @double => @double.ToJsObject(),
            float single => single.ToJsObject(),
            bool @bool => @bool.ToJsObject(),
            Action action => action.ToJsObject(),
            JSArray array => array.GetJsObject(),
            StringBuilder str => str.ToString().ToJsObject(),
            _ => obj
        };

        return obj;
    }

    public static object? FromJsObject(object? obj)
    {
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
    }
}