namespace REngine.Tools.AssetServer;

public static class ServerSettings
{
    public static string AssetsPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    public static string Host = "127.0.0.1";
    public static int Port = 80;
}