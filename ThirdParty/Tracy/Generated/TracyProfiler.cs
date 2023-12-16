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
        // var size32 = 2 + 4 + 4 + function.Length + 1 + source.Length + 1 + (name?.Length ?? 0);
        // if (size32 > ushort.MaxValue)
        //     throw new ArgumentOutOfRangeException(
        //         $"Can´t allocate source location because source, function or name is greater than {ushort.MaxValue}.");
        //
        // var size = (ushort)size32;
        // var data = new byte[size];
        //
        // // memcpy( ptr, &sz, 2 );
        // data[0] = (byte)(size & 0xFF);
        // data[1] = (byte)(size >> 8);
        // // memset( ptr + 2, 0, 4 );
        // data[2] = data[3] = data[4] = data[5] = 0;
        // // memcpy( ptr + 6, &line, 4 );
        // data[6] = (byte)(line & 0xFF);
        // data[7] = (byte)((line >> 8) & 0xFF);
        // data[8] = (byte)((line >> 16) & 0xFF);
        // data[9] = (byte)(line >> 24);
        // // memcpy( ptr + 10, function, functionSz );
        // var strBytes = Encoding.Default.GetBytes(function);
        // Array.Copy(strBytes, 0, data, 10, strBytes.Length);
        // // ptr[10 + functionSz] = '\0';
        // data[11] = 0;
        // strBytes = Encoding.Default.GetBytes(source);
        // Array.Copy(strBytes, 0, data, 10 + function.Length + 1, source.Length);
        // data[10 + function.Length + 1 + source.Length] = 0;
        // if(!string.IsNullOrEmpty(name))
        // {
        //     strBytes = Encoding.Default.GetBytes(name);
        //     Array.Copy(strBytes, 0, data, 10 + function.Length + source.Length + 1, strBytes.Length);
        // }
        //
        // var ptr = Marshal.AllocHGlobal(size);
        // var mappedData = new ReadOnlySpan<byte>(data);
        // fixed(byte* mappedDataPtr = mappedData)
        //     Buffer.MemoryCopy(mappedDataPtr, ptr.ToPointer(), size, size);
        // return (ulong)ptr.ToInt64();
        var sz32 = (uint)(2 + 4 + 4 + function.Length + 1 + source.Length + 1 + (string.IsNullOrEmpty(name) ? 0 : name.Length));
        if (sz32 > ushort.MaxValue)
            return 0;

        var sz = (ushort)sz32;
        var buffer = new byte[sz];

        buffer[0] = (byte)(sz & 0xFF);
        buffer[1] = (byte)(sz >> 8);

        BitConverter.GetBytes(line).CopyTo(buffer, 6);

        Encoding.UTF8.GetBytes(function).CopyTo(buffer, 10);
        buffer[10 + function.Length] = 0;

        Encoding.UTF8.GetBytes(source).CopyTo(buffer, 10 + function.Length + 1);
        buffer[10 + function.Length + 1 + source.Length] = 0;

        if (!string.IsNullOrEmpty(name))
        {
            Encoding.UTF8.GetBytes(name).CopyTo(buffer, 10 + function.Length + 1 + source.Length + 1);
        }
        
        var ptr = new IntPtr((long)PInvoke.TracyMalloc((ulong)buffer.Length));
        Marshal.Copy(buffer, 0, ptr, buffer.Length);

        var result = (ulong)ptr.ToInt64();
        return result;
    }
}