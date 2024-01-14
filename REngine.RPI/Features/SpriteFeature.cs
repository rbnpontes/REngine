using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Resources;
using REngine.RPI.Structs;

namespace REngine.RPI.Features;

public sealed class SpriteFeature  : GraphicsRenderFeature
{
    private readonly BatchGroup pBatchGroup;
    private readonly IBufferManager pBufferManager;
    private readonly Action<Batch> pExecuteBatchAction;
    public override bool IsDirty { get; protected set; } = true;

    public override IRenderFeature MarkAsDirty() => this;

    public SpriteFeature(BatchSystem batchSystem, IBufferManager bufferManager) : base()
    {
        pBufferManager = bufferManager;
        pBatchGroup = batchSystem.GetGroup(BatchGroupNames.Sprites);
        pExecuteBatchAction = ExecuteBatch;
    }
    
    protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
    {
        IsDirty = false;
    }

    private BatchRenderInfo pBatchRenderInfo;
    protected override void OnExecute(ICommandBuffer command)
    {
		var backbuffer = GetBackBuffer();
        var depthbuffer = GetDepthBuffer();

        if (backbuffer is null || depthbuffer is null)
            return;

        command.SetRT(backbuffer, depthbuffer);

        pBatchRenderInfo = new BatchRenderInfo
        {
            DefaultRenderTarget = backbuffer,
            DefaultDepthStencil = depthbuffer,
            CommandBuffer = command
        };
        
        pBatchGroup.Lock();
        pBatchGroup.Sort();
        pBatchGroup.ForEach(pExecuteBatchAction);
        pBatchGroup.Unlock();
    }

    private void ExecuteBatch(Batch batch)
    {
        batch.Render(pBatchRenderInfo);
    }
}