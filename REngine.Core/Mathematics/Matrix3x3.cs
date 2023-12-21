using System.Numerics;

namespace REngine.Core.Mathematics;

public struct Matrix3x3(
    float m11,
    float m12,
    float m13,
    float m21,
    float m22,
    float m23,
    float m31,
    float m32,
    float m33)
{
    public float M11 = m11;
    public float M12 = m12;
    public float M13 = m13;

    public float M21 = m21;
    public float M22 = m22;
    public float M23 = m23;

    public float M31 = m31;
    public float M32 = m32;
    public float M33 = m33;

    public Matrix3x3(
        Vector3 row1,
        Vector3 row2,
        Vector3 row3) : this(row1.X, row1.Y, row1.Z, row2.X, row2.Y, row2.Z, row3.X, row3.Y, row3.Z)
    {
    }

    public static Matrix3x3 operator *(Matrix3x3 matrix1, Matrix3x3 matrix2)
    {
        var m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31;
        var m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32;
        var m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33;

        var m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31;
        var m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32;
        var m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33;

        var m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31;
        var m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32;
        var m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33;

        return new Matrix3x3(
            m11, m12, m13,
            m21, m22, m23,
            m31, m32, m33
        );
    }

    public static Matrix3x3 CreateTranslation(Vector2 position)
    {
        return new Matrix3x3(
            1, 0, position.X,
            0, 1, position.Y,
            0, 0, 1
        );
    }

    public static Matrix3x3 CreateScale(Vector2 scale)
    {
        return new Matrix3x3(
            scale.X, 0, 0,
            0, scale.Y, 0,
            0, 0, 1
        );
    }

    public static Matrix3x3 CreateRotation(float angle)
    {
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);

        return new Matrix3x3(
            cos, -sin, 0,
            sin, cos, 0,
            0, 0, 1
        );
    }
    public static Matrix3x3 CreateTransform(Vector2 position, float rotation, Vector2 size)
    {
        var rotationMatrix = CreateRotation(rotation);
        // var translationMatrix = CreateTranslation(position);
        // var rotationMatrix = CreateRotation(rotation);
        // var scaleMatrix = CreateScale(size);
        // return (rotationMatrix * translationMatrix) * scaleMatrix;
        return new Matrix3x3(
            rotationMatrix.M11 * size.X, rotationMatrix.M12 * size.Y, position.X,
            rotationMatrix.M21 * size.X, rotationMatrix.M22 * size.Y, position.Y,
            0, 0, 1
        );
    }

    public static readonly Matrix3x3 Identity = new(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1
    );

    public static readonly Matrix3x3 Zero = new();
}