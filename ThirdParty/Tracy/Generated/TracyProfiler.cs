using System.Runtime.InteropServices;
using System.Text;

namespace Tracy.Generated;

/// <summary>
/// Rewrited Tracy Methods on C#
/// </summary>
public static class TracyProfiler
{
    public static unsafe ulong AllocSourceLocation(int line, string source, string function, string? name = null)
    {
        // var sz32 = (uint)(2 + 4 + 4 + function.Length + 1 + source.Length + 1 + (string.IsNullOrEmpty(name) ? 0 : name.Length));
        // if (sz32 > ushort.MaxValue)
        //     return 0;
        //
        // var sz = (ushort)sz32;
        // var buffer = new byte[sz];
        //
        // buffer[0] = (byte)(sz & 0xFF);
        // buffer[1] = (byte)(sz >> 8);
        //
        // BitConverter.GetBytes(line).CopyTo(buffer, 6);
        //
        // Encoding.UTF8.GetBytes(function).CopyTo(buffer, 10);
        // buffer[10 + function.Length] = 0;
        //
        // Encoding.UTF8.GetBytes(source).CopyTo(buffer, 10 + function.Length + 1);
        // buffer[10 + function.Length + 1 + source.Length] = 0;
        //
        // if (!string.IsNullOrEmpty(name))
        // {
        //     Encoding.UTF8.GetBytes(name).CopyTo(buffer, 10 + function.Length + 1 + source.Length + 1);
        // }
        //
        // var ptr = new IntPtr((long)PInvoke.TracyMalloc((ulong)buffer.Length));
        // Marshal.Copy(buffer, 0, ptr, buffer.Length);
        //
        // var result = (ulong)ptr.ToInt64();
        // return result;
        
        var sz32 = (uint)(2 + 4 + 4 + function.Length + 1 + source.Length + 1 + (string.IsNullOrEmpty(name) ? 0 : name.Length));
        if (sz32 > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(
                $"function, Source or Name Lengths is greater than {ushort.MaxValue}. Function={function}, Source={source.Length} Name={name} Size={sz32}");

        var sz = (ushort)sz32;
        var ptr = (IntPtr)PInvoke.TracyMalloc(sz);

        *((ushort*)ptr) = sz;
        *((int*)(ptr + 6)) = line;

        var span = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(function));
        fixed (byte* funcPtr = span)
            Buffer.MemoryCopy(funcPtr, (ptr + 10).ToPointer(), function.Length, function.Length);
        *((byte*)(ptr + 10 + function.Length)) = 0;

        span = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(source));
        fixed (byte* sourcePtr = span)
            Buffer.MemoryCopy(sourcePtr, (ptr + 10 + function.Length + 1).ToPointer(), source.Length, source.Length);
        *((byte*)(ptr + 10 + function.Length + 1 + source.Length)) = 0;

        if (!string.IsNullOrEmpty(name))
        {
            span = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(name));
            fixed(char* namePtr = name)
                Buffer.MemoryCopy(namePtr, (ptr + 10 + function.Length + 1 + source.Length + 1).ToPointer(), name.Length, name.Length);
        }

        return (ulong)ptr.ToInt64();
    }
}