using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Resources;
using REngine.RPI.Structs;

namespace REngine.RPI.Features;

public sealed class SpriteFeature(
    BatchSystem batchSystem,
    SpriteInstancedBatchSystem instancedBatchSys,
    IShaderResourceBindingCache srbCache,
    IBufferManager bufferMgr) : GraphicsRenderFeature
{
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(SpriteSystem.BatchGroupName);
    
    public override bool IsDirty { get; protected set; } = true;

    public override IRenderFeature MarkAsDirty() => this;
    protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
    {
        IsDirty = false;
    }

    protected override void OnExecute(ICommandBuffer command)
    {
		instancedBatchSys.UpdateTransforms();

		var backbuffer = GetBackBuffer();
        var depthbuffer = GetDepthBuffer();

        if (backbuffer is null || depthbuffer is null)
            return;

        command.SetRT(backbuffer, depthbuffer);

        var batchRenderInfo = new BatchRenderInfo
        {
            DefaultRenderTarget = backbuffer,
            DefaultDepthStencil = depthbuffer,
            CommandBuffer = command
        };
        
        pBatchGroup.Lock();
        pBatchGroup.Sort();
        foreach (var batch in pBatchGroup)
            batch.Render(batchRenderInfo);
        pBatchGroup.Unlock();
    }
}