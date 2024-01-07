using System.Numerics;

namespace REngine.Core.Mathematics;

public struct Matrix2x2
{
    public float M11;
    public float M12;
    public float M21;
    public float M22;
    
    public Matrix2x2() {}

    public Matrix2x2(float m11, float m12, float m21, float m22)
    {
        M11 = m11;
        M12 = m12;
        M21 = m21;
        M22 = m22;
    }

    public Matrix2x2(Vector2 firstRow, Vector2 secondRow)
    {
        M11 = firstRow.X;
        M12 = firstRow.Y;
        M21 = secondRow.X;
        M22 = secondRow.Y;
    }
    
    public override string ToString()
    {
        return $"M11: {M11}, M12: {M12}\nM21: {M21}, M22: {M22}";
    }

    public static Matrix2x2 operator *(Matrix2x2 matrix1, Matrix2x2 matrix2)
    {
        var m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21;
        var m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22;
        
        var m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21;
        var m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22;

        return new Matrix2x2(m11, m12, m21, m22);
    }
    public static Matrix2x2 CreateTranslation(Vector2 position)
    {
        return new Matrix2x2(position.X, 0, 0, position.Y);
    }
    public static Matrix2x2 CreateScale(Vector2 scale)
    {
        return CreateTranslation(scale);
    }
    public static Matrix2x2 CreateRotation(float angleRadians)
    {
        var cos = (float)Math.Cos(angleRadians);
        var sin = (float)Math.Sin(angleRadians);
        return new Matrix2x2(
            cos, -sin,
            sin, cos
        );
    }
    
    public static readonly Matrix2x2 Zero = new Matrix2x2();
    public static readonly Matrix2x2 Identity = new Matrix2x2(1, 0, 0, 1);
}