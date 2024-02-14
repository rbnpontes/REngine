namespace REngine.Core.Mathematics;

public struct IntVector3(int x = 0, int y = 0, int z = 0)
{
    public int X = x;
    public int Y = y;
    public int Z = z;

    public static readonly IntVector3 Zero = new();
    public static readonly IntVector3 One = new(1, 1, 1);
}