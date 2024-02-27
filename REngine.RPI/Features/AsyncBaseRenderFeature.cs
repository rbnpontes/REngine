using MathNet.Numerics.Optimization;
using REngine.Core;
using REngine.Core.Threading;
using REngine.RHI;

namespace REngine.RPI.Features;

public abstract class AsyncBaseRenderFeature(
    IRenderer renderer,
    IExecutionPipeline executionPipeline) : IRenderFeature
{
    private readonly object pSync = new();
    private readonly CancellationTokenSource pTokenSource = new();
    
    private uint pDirtyCalls = 0;

    public bool IsDirty { get; private set; } = true;
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        if(IsDisposed)
            return;
        
        if(!pTokenSource.IsCancellationRequested)
            pTokenSource.Cancel();
        
        lock (pSync)
        {
            // If setup is still running, then we must wait before continue
            // In this case, we enqueue dispose of this object on the next frame
            if (pDirtyCalls > 0)
            {
                DisposableQueue.Enqueue(this);
                return;
            }
        }

        IsDirty = false;
        IsDisposed = true;
        OnDispose();
        GC.SuppressFinalize(this);
    }
    
    public IRenderFeature MarkAsDirty()
    {
        if (pTokenSource.IsCancellationRequested)
            return this;
        IsDirty = true;
        lock (pSync)
            ++pDirtyCalls;
        return this;
    }

    public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo)
    {
        if (IsDisposed)
            return this;
        // If previous setup is still running
        lock (pSync)
        {
            if (pDirtyCalls > 0)
                return this;
            ++pDirtyCalls;
        }

        var execData = execInfo;
        Task.Run(async () =>
        {
            try
            {
                await HandleSetup(execData);
            }
            catch (Exception ex)
            {
                executionPipeline.Invoke(() => throw new Exception($"Error has occurred while is setup {nameof(AsyncBaseRenderFeature)}.", ex));
            }
        });
        return this;
    }

    public IRenderFeature Compile(ICommandBuffer command)
    {
        // Skip Compile Step on AsyncBaseRenderFeature
        return this;
    }

    public IRenderFeature Execute(ICommandBuffer command)
    {
        if (IsDisposed)
            return this;
        OnExecute(command);
        return this;
    }

    private async Task HandleSetup(RenderFeatureSetupInfo execInfo)
    {
        if (IsDisposedTriggered())
            return;

        await OnSetup(execInfo);
        
        if(IsDisposedTriggered())
            return;
        
        await OnCompile(execInfo.Driver.ImmediateCommand);
        
        if(IsDisposedTriggered())
            return;

        lock (pSync)
        {
            --pDirtyCalls;
            if (pDirtyCalls > 0)
            {
                pDirtyCalls = 1;
                return;
            }
        }

        IsDirty = false;
        return;

        bool IsDisposedTriggered()
        {
            if (!pTokenSource.IsCancellationRequested)
                return false;

            lock (pSync)
                pDirtyCalls = 0;
            IsDirty = false;
            return true;
        }
    }
    
    protected virtual async Task OnSetup(RenderFeatureSetupInfo execInfo)
    {
        await Task.Yield();
    }

    protected virtual async Task OnCompile(ICommandBuffer command)
    {
        await Task.Yield();
    }
    
    protected virtual void OnExecute(ICommandBuffer command) {}
    protected virtual void OnDispose() {}
    
    protected virtual ITextureView? GetBackBuffer()
    {
        return renderer.SwapChain?.ColorBuffer;
    }
    protected virtual ITextureView? GetDepthBuffer()
    {
        return renderer.SwapChain?.DepthBuffer;
    }
}

public abstract class AsyncGraphicsRenderFeature(IRenderer renderer, IExecutionPipeline executionPipeline) : AsyncBaseRenderFeature(renderer, executionPipeline)
{
    public ITextureView? BackBuffer { get; set; }
    public ITextureView? DepthBuffer { get; set; }

    protected override ITextureView? GetBackBuffer()
    {
        return BackBuffer ?? base.GetBackBuffer();
    }
    protected override ITextureView? GetDepthBuffer()
    {
        return DepthBuffer ?? base.GetDepthBuffer();
    }
}