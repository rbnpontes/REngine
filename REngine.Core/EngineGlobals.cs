using REngine.Core.IO;
using REngine.Core.Threading;

namespace REngine.Core;

#nullable disable
public static class EngineGlobals
{
    public static IDispatcher MainDispatcher { get; internal set; }
    public static IServiceProvider ServiceProvider { get; internal set; }
    public static ILoggerFactory LoggerFactory { get; internal set; }
}