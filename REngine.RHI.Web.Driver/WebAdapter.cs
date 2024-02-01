namespace REngine.RHI.Web.Driver;

internal class WebAdapter : IGraphicsAdapter
{
    public ulong ToHash()
    {
        return 0;
    }

    public uint Id => 0;
    public uint DeviceId => 0;
    public uint VendorId => 0;
    public string Name => "WebAdapter;WebGL";
    public AdapterType AdapterType => AdapterType.Unknow;
    public ulong LocalMemory => 0;
    public ulong HostVisibleMemory => 0;
    public ulong UnifiedMemory => 0;
    public ulong MaxMemoryAlloc => 0;
    public CpuAccessFlags UnifiedMemoryCpuAccess => CpuAccessFlags.Write | CpuAccessFlags.Read;
    public BindFlags MemorylessTextureBindFlags => BindFlags.None;

    public static readonly WebAdapter Default = new WebAdapter();
}