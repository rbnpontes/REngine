namespace REngine.RHI.NativeDriver;

internal partial class CommandBufferImpl
{
    private const string SetRtName = $"{nameof(ICommandBuffer)}.{nameof(SetRT)}";
    private const string SetViewportsName = $"{nameof(ICommandBuffer)}.{nameof(SetViewports)}";
    private const string ClearRtName = $"{nameof(ICommandBuffer)}.{nameof(ClearRT)}";
    private const string ClearDepthName = $"{nameof(ICommandBuffer)}.{nameof(ClearDepthName)}";
}