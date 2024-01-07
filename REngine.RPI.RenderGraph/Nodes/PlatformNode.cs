using REngine.Core;
using REngine.Core.Mathematics;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.Nodes;

[NodeTag("platform")]
public sealed class PlatformNode() : RenderGraphNode(nameof(PlatformNode))
{
    private const string NamePropertyKey = "name";
    private readonly ulong NamePropertyHash = Hash.Digest(NamePropertyKey);
    private bool pEnabled;
    

    protected override void OnSetup(IDictionary<ulong, string> properties)
    {
        if (!properties.TryGetValue(NamePropertyHash, out var name))
            throw new RequiredNodePropertyException(NamePropertyKey, nameof(PlatformNode));

        var platformId = Hash.Digest(name);
        pEnabled = Platform.IsTargetPlatform(platformId);
    }

    protected override void OnRun(IServiceProvider provider)
    {
    }

    protected override IReadOnlyList<RenderGraphNode> OnGetChildren()
    {
        return pEnabled ? base.OnGetChildren() : [];
    }
}