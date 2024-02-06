using System.Buffers;
using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver;

internal partial class CommandBufferImpl
{
    private IntPtr pDriverMem = NativeApis.js_malloc((int)DriverSettings.CommandBufferMemorySize);
    private ArrayPool<byte> pPool = ArrayPool<byte>.Shared;
    private byte[] pMappedData = [];
    private GCHandle? pPinnedHandle;
    private IntPtr pMappedPtr = IntPtr.Zero;
    
    private float[] pFloatArray = new float[4];
}