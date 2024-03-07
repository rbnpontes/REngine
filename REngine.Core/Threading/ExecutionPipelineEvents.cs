using REngine.Core.Threading;

namespace REngine.Core.Events;

public sealed class ExecutionPipelineEvents
{
    public event EventHandler? OnDispose;
    public event EventHandler? OnLoad;
    public event EventHandler? OnLoaded;

    public ExecutionPipelineEvents ExecuteDispose(IExecutionPipeline execPipeline)
    {
        OnDispose?.Invoke(execPipeline, EventArgs.Empty);
        return this;
    }

    public ExecutionPipelineEvents ExecuteLoad(IExecutionPipeline execPipeline)
    {
        OnLoad?.Invoke(execPipeline, EventArgs.Empty);
        return this;
    }

    public ExecutionPipelineEvents ExecuteLoaded(IExecutionPipeline execPipeline)
    {
        OnLoaded?.Invoke(execPipeline, EventArgs.Empty);
        return this;
    }
}