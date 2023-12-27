namespace REngine.RHI.NativeDriver;

internal class CommandListImpl(IntPtr handle) : NativeObject(handle), ICommandList
{
    public GPUObjectType ObjectType => GPUObjectType.Unknown;
    public string Name => nameof(ICommandList);
}