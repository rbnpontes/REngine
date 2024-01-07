namespace REngine.Core.Utils;

public static class ExceptionUtils
{
    public static void ThrowIfOutOfBounds(int idx, int length)
    {
        if (idx >= 0 && idx < length)
            return;
        throw new IndexOutOfRangeException();
    }
}