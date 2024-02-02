namespace REngine.RHI.Web.Driver.Models;

internal unsafe struct DriverData
{
    public IntPtr Device;
    public IntPtr Context;
    public IntPtr Factory;

    public ref DriverData GetPinnableReference()
    {
        return ref this;
    }
}