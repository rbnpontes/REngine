namespace REngine.Tools.AssetServer;

public class Parameters
{
    private readonly Dictionary<string, string> pParams = new();
    
    public void Collect(string[] args)
    {
        var param = string.Empty;
        for (var i = 0; i < args.Length; ++i)
        {
            if (i % 2 == 0)
                param = args[i];
            else
                pParams[param] = args[i];
        }    
    }

    public string? GetParam(string paramKey)
    {
        pParams.TryGetValue(paramKey, out var value);
        return value;
    }
}