namespace REngine.RHI.Web.Driver.Models;

internal unsafe struct BufferDescDto
{
     public IntPtr Name;
     public ulong Size;
     public uint BindFlags;
     public byte Usage;
     public byte AccessFlags;
     public byte Mode;
     public uint ElementByteStride;

     public BufferDescDto()
     {
          Name = IntPtr.Zero;
          Size = 0;
          BindFlags = 0;
          Usage = AccessFlags = Mode = 0;
          ElementByteStride = 0;
     }

     public BufferDescDto(in BufferDesc desc)
     {
          Name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : NativeApis.js_alloc_string(desc.Name);
          Size = desc.Size;
          BindFlags = (uint)desc.BindFlags;
          Usage = (byte)desc.Usage;
          AccessFlags = (byte)desc.AccessFlags;
          Mode = (byte)desc.Mode;
          ElementByteStride = desc.ElementByteStride;
     }

     public ref BufferDescDto GetPinnableReference()
     {
          return ref this;
     }
}