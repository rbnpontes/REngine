using REngine.Core.IO;

namespace REngine.Core.Android;

public sealed class AndroidLoggerFactory : BaseLoggerFactory, ILoggerFactory
{
    public ILogger<T> Build<T>()
    {
        return new Logger<T>(this);
    }

    public ILogger Build(Type genericType)
    {
        return new NonGenericLogger(this, genericType);
    }

    public ILoggerFactory Log(LogSeverity severity, string tag, object[] args)
    {
        var log = BuildLog(severity, tag, args);
        switch (severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                global::Android.Util.Log.Error(tag,log);
                break;
            case LogSeverity.Warning:
                global::Android.Util.Log.Warn(tag, log);
                break;
            case LogSeverity.Success:
                global::Android.Util.Log.Info(tag, log);
                break;
            case LogSeverity.Debug:
                global::Android.Util.Log.Debug(tag, log);
                break;
            case LogSeverity.Info:
                global::Android.Util.Log.Info(tag, log);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
        }

        return this;
    }
}