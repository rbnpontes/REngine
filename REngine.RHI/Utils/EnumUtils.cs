namespace REngine.RHI.Utils;

public static class EnumUtils
{
    public static ShaderTypeFlags GetShaderTypeFlags(ShaderType shaderType)
    {
        return shaderType switch
        {
            ShaderType.Vertex => ShaderTypeFlags.Vertex,
            ShaderType.Pixel => ShaderTypeFlags.Pixel,
            ShaderType.Hull => ShaderTypeFlags.Hull,
            ShaderType.Geometry => ShaderTypeFlags.Geometry,
            ShaderType.Domain => ShaderTypeFlags.Domain,
            ShaderType.Compute => ShaderTypeFlags.Compute,
            _ => ShaderTypeFlags.None
        };
    }
}