using System.Numerics;

namespace REngine.RPI.Utils;

public static class MatrixUtils
{
    public static Matrix4x4 GetSpriteTransform(Vector3 position, Vector2 anchor, float rotation, Vector2 scale)
    {
        var transform = Matrix4x4.CreateScale(new Vector3(scale, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3((scale * anchor) * new Vector2(-1), 0));
        return transform * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(position);
    }
}