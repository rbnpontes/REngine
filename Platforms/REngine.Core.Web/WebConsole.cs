namespace REngine.Core.Web;

internal enum LogType
{
    Log,
    Warning,
    Error
}
public static partial class WebConsole
{
    public static void Log(params object[] args) => ExecuteLog(LogType.Log, args);

    public static void Warn(params object[] args) => ExecuteLog(LogType.Warning, args);

    public static void Error(params object[] args) => ExecuteLog(LogType.Error, args);

    private static void ExecuteLog(LogType type, object[] args)
    {
        var arr = new JSArray();
        foreach (var arg in args)
            arr.Add(arg);

        switch (type)
        {
            case LogType.Log:
                js_console_log(arr.GetId());
                break;
            case LogType.Warning:
                js_console_warn(arr.GetId());
                break;
            case LogType.Error:
                js_console_error(arr.GetId());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}