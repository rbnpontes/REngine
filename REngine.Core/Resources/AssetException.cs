namespace REngine.Core.Resources;

public class AssetException(string msg) : Exception(msg);
public class NotFoundAssetException(string assetName) : AssetException($"Not found asset '{assetName}'");

public class HttpAssetMetadataDownload(string metadataUrl) : AssetException($"Could not possible to download metadata at '{metadataUrl}' address. It seems this address is not reachable!"){}

public class HttpAssetMetadataParse(string data) : AssetException($"Error has occurred while is parsing metadata file.")
{
    public string Metadata { get; } = data;
}
public class HttpAssetDownload(string url) : AssetException($"Error has occurred while is downloading '{url}'.") {}