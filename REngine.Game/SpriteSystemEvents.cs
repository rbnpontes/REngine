using REngine.Core.Mathematics;

namespace REngine.Game;

public static class SpriteSystemEvents
{
    public static ulong EndUpdate = Hash.Digest("@engine/spritesys/end_update");
    public static ulong Render = Hash.Digest("@engine/spritesys/render");
}