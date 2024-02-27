using REngine.Core.IO;
using REngine.Core.Web;

namespace REngine.Core.IO;

public sealed class WebLoggerFactory : BaseLoggerFactory, ILoggerFactory
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
                WebConsole.Error(log, Environment.StackTrace);
                break;
            case LogSeverity.Warning:
                WebConsole.Warn(log, Environment.StackTrace);
                break;
            case LogSeverity.Success:
                WebConsole.Log(log, "color: green");
                break;
            case LogSeverity.Debug:
                WebConsole.Debug(log);
                break;
            case LogSeverity.Info:
                WebConsole.Log(log);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
        }
        return this;
    }
}