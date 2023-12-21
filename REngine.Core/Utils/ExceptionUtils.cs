namespace REngine.Core.Utils;

public static class ExceptionUtils
{
    public static void ThrowIfOutOfBounds(int idx, int length)
    {
        if (idx < length)
            return;
        throw new IndexOutOfRangeException();
    }
}