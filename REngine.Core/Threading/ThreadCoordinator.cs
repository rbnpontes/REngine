using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using REngine.Core.Mathematics;

namespace REngine.Core.Threading
{
	//TODO: enable this when .NET team implements Multithreading support
#if !WEB
	internal class ThreadCoordinator(ILoggerFactory loggerFactory) : IDisposable
	{
		private readonly ConcurrentQueue<Action> pActions = new();
		private readonly CancellationTokenSource pCancellationTokenSource = new();
		private readonly ILogger<ThreadCoordinator> pLogger = loggerFactory.Build<ThreadCoordinator>();
		private readonly object pLock = new();

		private Thread[] pThreads = Array.Empty<Thread>();
		private int pThreadSleepMs;
		private bool pStarted;
		private bool pDisposed;

		public int JobsCount => pThreads.Length;
		public void Dispose()
		{
			if (pDisposed)
				return;
			pLogger.Debug("Stopping Thread Coordinator");
			pCancellationTokenSource.Cancel();

			pLogger.Debug("Waiting Threads to Finish");
			foreach (var t in pThreads)
			{
				t.Join();
				pLogger.Debug($"Finished {t.Name}");
			}

			pThreads = [];
			pDisposed = true;
		}

		public void Start(int jobsCount)
		{
			if (pStarted)
				return;

			pLogger.Debug("Starting Thread Coordinator");

			pThreads = new Thread[jobsCount];

			pLogger.Debug($"Creating {pThreads.Length} Threads.");

			for (var i = 0; i < pThreads.Length; i++)
			{
				pThreads[i] = new Thread(ThreadExecution)
				{
					Name = $"REngine - Worker #{i}"
				};
				pThreads[i].Start();
			}

			lock (pLock)
				pStarted = true;

			pLogger.Debug("Started");
		}

		public void SetThreadSleep(int sleepMs)
		{
			if (pThreadSleepMs == sleepMs)
				return;
			Interlocked.Exchange(ref pThreadSleepMs, sleepMs);
		}

		public void EnqueueAction(Action action)
		{
			pActions.Enqueue(action);
		}

		private void ThreadExecution()
		{
			while (true)
			{
				bool started;
				lock (pLock)
					started = pStarted;
				if (started)
					break;
			}

			var token = pCancellationTokenSource.Token;
			var threadName = Thread.CurrentThread.Name ?? nameof(ThreadCoordinator);
			while (!token.IsCancellationRequested)
			{
#if PROFILER
				Profiler.Instance.BeginTask(threadName);
#endif
				if (pActions.TryDequeue(out var action))
				{
					if (token.IsCancellationRequested)
						break;
					action();
				}

#if RENGINE_DEBUGLOCKS
				if (Monitor.GetLockCount() != 0)
				{
					var threadId = Hash.Digest(Thread.CurrentThread.Name ?? "Unknown Thread");
					var output = new StringBuilder();
					Monitor.Dump(output);
					pLogger.Critical(
						$"There's mutex that has not been released: \nCurrent Thread: {threadId}\nOutput: {output}");
				}
#endif
				var threadSleepMs = pThreadSleepMs;
				if(threadSleepMs > 0)
					Thread.Sleep(threadSleepMs);
				
#if PROFILER
				Profiler.Instance.EndTask(threadName);
#endif
			}
		}
	}
#endif
}
