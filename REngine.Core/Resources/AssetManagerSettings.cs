using REngine.Core.Serialization;

namespace REngine.Core.Resources;

public class FileAssetManagerSettings
{
     public string[] SearchPaths { get; set; } =
     [
          Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REngine")
     ];
}

public class HttpAssetManagerSettings
{
     public string MetadataUrl { get; set; } = "http://127.0.0.1/metadata";
}
public class AssetManagerSettings
{
     public FileAssetManagerSettings? FileSettings { get; set; } = new();
     public HttpAssetManagerSettings? HttpSettings { get; set; } = new();

     public static AssetManagerSettings FromStream(Stream stream)
     {
          AssetManagerSettings? settings;
          using (TextReader reader = new StreamReader(stream))
               settings = reader.ReadToEnd().FromJson<AssetManagerSettings>();
          return settings ?? new AssetManagerSettings();
     }
}