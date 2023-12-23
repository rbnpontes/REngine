using REngine.Core;
using REngine.Core.Mathematics;

namespace REngine.RHI;

public sealed class ResourceMappingEntry(
    string name, ShaderType type, IGPUObject obj) : IHashable
{
    public string Name => name;
    public ShaderType Type => type;
    public IGPUObject GPUObject => obj;
    public ulong ToHash()
    {
        var hash = Hash.Digest(Name);
        hash = Hash.Combine(hash, (byte)type);
        hash = Hash.Combine(hash, (ulong)GPUObject.Handle);
        return hash;
    }
}
public sealed class ResourceMapping : IHashable
{
    private readonly Dictionary<ulong, ResourceMappingEntry> pEntries = new();

    public ResourceMapping Add(ShaderTypeFlags flags, string name, IGPUObject obj)
    {
        if((flags & ShaderTypeFlags.Vertex) != 0)
            InsertResource(ShaderType.Vertex, name, obj);
        if((flags & ShaderTypeFlags.Pixel) != 0)
            InsertResource(ShaderType.Pixel, name, obj);
        if((flags & ShaderTypeFlags.Compute) != 0)
            InsertResource(ShaderType.Compute, name, obj);
        if((flags & ShaderTypeFlags.Geometry) != 0)
            InsertResource(ShaderType.Geometry, name, obj);
        if((flags & ShaderTypeFlags.Hull) != 0)
            InsertResource(ShaderType.Hull, name, obj);
        if((flags & ShaderTypeFlags.Domain) != 0)
            InsertResource(ShaderType.Domain, name, obj);
        return this;
    }

    public ResourceMapping Clear()
    {
        pEntries.Clear();
        return this;
    }

    public ResourceMappingEntry[] GetEntries()
    {
        return pEntries.Values.ToArray();
    }
    
    private void InsertResource(ShaderType type, string name, IGPUObject obj)
    {
        if (obj is ITexture texture)
            obj = texture.GetDefaultView(TextureViewType.ShaderResource);
        var entry = new ResourceMappingEntry(name, type, obj);
        pEntries.TryAdd(entry.ToHash(), entry);
    }

    public ulong ToHash()
    {
        return pEntries.Aggregate<KeyValuePair<ulong, ResourceMappingEntry>, ulong>(1, (current, res) => Hash.Combine(current, res.Key));
    }
}