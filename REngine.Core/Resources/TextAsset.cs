namespace REngine.Core.Resources;

public class TextAsset : Asset
{
    public string Text { get; private set; } = string.Empty;
    protected override void OnLoad(AssetStream stream)
    {
        using TextReader reader = new StreamReader(stream);
        OnLoadText(reader.ReadToEnd());
    }

    protected virtual void OnLoadText(string data)
    {
        Text = data;
    }
    
    protected override void OnDispose()
    {
    }
}