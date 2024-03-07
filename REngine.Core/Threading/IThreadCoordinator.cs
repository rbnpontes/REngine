namespace REngine.Core.Threading;

public interface IThreadCoordinator
{
     public int JobsCount { get; }
     public bool IsJobThread { get; }
     public void Start(int jobsCount);
     public void SetThreadSleep(int sleepMs);
     public void EnqueueAction(Action action);
}