using System.Net.Sockets;
using NetCoreServer;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Serialization;

namespace REngine.Tools.AssetServer;

public class Session(ILoggerFactory loggerFactory, HttpServer server) : HttpSession(server)
{
    private readonly ulong pMetadataRoute = Hash.Digest("/metadata");
    private readonly ILogger<Session> pLogger = loggerFactory.Build<Session>();
    private readonly Dictionary<string, string> pAssets = new();

    protected override void OnReceivedRequest(HttpRequest request)
    {
        if (request.Method == "HEAD")
        {
            SendResponseAsync(Response.MakeHeadResponse());
            return;
        }

        if (request.Method == "GET")
        {
            var path = Uri.UnescapeDataString(request.Url);
            var key = Hash.Digest(path);

            pLogger.Info($"Loading: {path}");
            if (key == pMetadataRoute)
            {
                WalkAndCollectAssets(ServerSettings.AssetsPath);
                var assets = new Dictionary<string, string>();
                foreach (var pair in pAssets)
                    assets[pair.Key] = $"http://{ServerSettings.Host}:{ServerSettings.Port}/Assets/{pair.Key}";
                
                SendResponseAsync(Response.MakeGetResponse(assets.ToJson(), "application/json"));
                return;
            }
            
        }

        SendResponseAsync(Response.MakeOkResponse());
    }

    private void WalkAndCollectAssets(string path)
    {
        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            var assetPath = file.Replace(ServerSettings.AssetsPath, string.Empty);
            if (assetPath.StartsWith('/') || assetPath.StartsWith('\\'))
                assetPath = assetPath[1..];
            assetPath = assetPath.Replace("\\", "/");
            pAssets[assetPath] = file;
        }
        
        foreach(var dir in directories)
            WalkAndCollectAssets(dir);
    }
    
    protected override void OnReceivedRequestError(HttpRequest request, string error)
    {
        pLogger.Error(error);
    }

    protected override void OnError(SocketError error)
    {
        pLogger.Error(error);
    }
}