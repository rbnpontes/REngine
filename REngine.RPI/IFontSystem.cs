using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI;

public interface IFontSystem
{
    public bool HasPendingFonts { get; }
    public void SetFont(Font font);
    public Font? GetFont(string fontName);
    public Font? GetFont(ulong fontId);
    public ITexture? GetFontAtlas(string fontName);
    public ITexture? GetFontAtlas(ulong fontId);
    public void ClearFont(string fontName);
    public void ClearFont(ulong fontId);
    public void ClearAllFonts();
}