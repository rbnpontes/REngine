using REngine.Core.DependencyInjection;

namespace REngine.Core.Threading;

internal static class ThreadingModule
{
    public static void Setup(IServiceRegistry registry)
    {
        registry
            .Add<IThreadCoordinator, ThreadCoordinatorImpl>()
            .Add<IExecutionPipeline, ExecutionPipelineImpl>()
            .Add<ExecutionPipelineNodeRegistry>();
    }
}