// See https://aka.ms/new-console-template for more information

using System.Text;
using REngine.Core;
using REngine.Core.IO;
using REngine.Tools.AssetServer;

if (args.Contains("--help") || args.Contains("-h"))
{
    StringBuilder builder = new();
    builder.Append("::::::: REngine - Asset Server Help Commands :::::::");
    builder.AppendLine("--help, -h\t\t\tHelp Commands");
    builder.AppendLine("-src, --source\t\t\tPhysical Path of Assets to be served");
    builder.AppendLine("-addr, --address\t\t\tAddress of Server (default=127.0.0.1)");
    builder.AppendLine("-p, --port\t\t\tPort of Server (default=80)");
    return;
}
    
    
var parameters = new Parameters();
parameters.Collect(args);

// Collect Parameters
var assetPath = parameters.GetParam("-src") ?? parameters.GetParam("--source");
var addr = parameters.GetParam("-addr") ?? parameters.GetParam("--address");
var port = parameters.GetParam("-p") ?? parameters.GetParam("--port") ?? "80";

ServerSettings.AssetsPath = Path.GetFullPath(assetPath ?? AppDomain.CurrentDomain.BaseDirectory);
ServerSettings.Host = addr ?? "127.0.0.1";
ServerSettings.Port = int.Parse(port);

var loggerFactory = new DebugLoggerFactory();
var server = new Server(loggerFactory,  ServerSettings.Host, ServerSettings.Port);
server.AddStaticContent(ServerSettings.AssetsPath, "/Assets");
server.Start();

Console.WriteLine($"Server Started at {ServerSettings.Host}:{ServerSettings.Port}");
Console.WriteLine($"Serving Contents Of: {ServerSettings.AssetsPath}");
Console.WriteLine("Press 'Enter' to Close or 'R' to Restart");

while (true)
{
    var line = Console.ReadLine();
    if (Equals(line, string.Empty))
    {
        Console.WriteLine("Stopping Server");
        break;
    }

    if (Equals(line, "R") || Equals(line, "r"))
    {
        Console.WriteLine("Restarting Server");
        server.Stop();
        server.Dispose();

        server = new Server(loggerFactory, ServerSettings.Host, ServerSettings.Port);
        server.AddStaticContent(ServerSettings.AssetsPath, "/Assets");
        server.Start();
        
        Console.WriteLine("Server restarted with success");
    }
}

server.Stop();
Console.WriteLine("Asset Server is Stopped");