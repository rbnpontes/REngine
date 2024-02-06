namespace REngine.RHI.Web.Driver;

public static class DriverSettings
{
    /**
     * Driver prevents unnecessary allocations
     * to achieve high performance
     * When CommandBuffer is created, an block of memory
     * is created on the driver side.
     * Then all necessary info is on this memory and executes on driver
     * This memory is not shared, is used at each process
     * so eventually is written by other commands.
     * The value above specify the size of this memory
     */
    public const uint CommandBufferMemorySize = 2048;
}