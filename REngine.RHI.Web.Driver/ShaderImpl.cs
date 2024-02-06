namespace REngine.RHI.Web.Driver;

internal class ShaderImpl(IntPtr handle, ShaderCreateInfo createInfo) : NativeObject(handle), IShader
{
    public GPUObjectType ObjectType => GPUObjectType.Shader;
    public string Name => createInfo.Name;
    public ShaderType Type => createInfo.Type;
    public ulong ToHash() => createInfo.ToHash();
}