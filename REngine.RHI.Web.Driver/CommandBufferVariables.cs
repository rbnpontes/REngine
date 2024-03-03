using System.Buffers;
using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver;

internal partial class CommandBufferImpl
{
    private class MappedData
    {
        public IntPtr Data = IntPtr.Zero;
        public IntPtr DriverData = IntPtr.Zero;
        public MapFlags Flags;
    }
    
    private readonly IntPtr pDriverMem = NativeApis.js_malloc((int)DriverSettings.CommandBufferMemorySize);
    private Dictionary<IntPtr, MappedData> pMappedDataMap = new();
    
    private float[] pFloatArray = new float[4];
}