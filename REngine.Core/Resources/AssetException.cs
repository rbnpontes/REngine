namespace REngine.Core.Resources;

public class AssetException(string msg) : Exception(msg);
public class NotFoundAssetException(string assetName) : AssetException($"Not found asset '{assetName}'");