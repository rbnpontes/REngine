using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using REngine.Core.IO;

namespace REngine.Tools.AssetServer;

public class Server(ILoggerFactory loggerFactory, string address, int port) : HttpServer(address, port)
{
    private readonly ILogger<Server> pLogger = loggerFactory.Build<Server>();
    protected override TcpSession CreateSession() => new Session(loggerFactory, this);

    protected override void OnError(SocketError error)
    {
        pLogger.Error(error);
    }
}