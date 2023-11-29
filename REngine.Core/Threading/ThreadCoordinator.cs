using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;

namespace REngine.Core.Threading
{
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
					Name = $"Worker #{i}"
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
					try
					{
						action();
					}
					catch (Exception ex)
					{
						pLogger.Error(ex);
					}
				}

				var threadSleepMs = pThreadSleepMs;
				if(threadSleepMs > 0)
					Thread.Sleep(threadSleepMs);
#if PROFILER
				Profiler.Instance.EndTask(threadName);
#endif
			}
		}
	}
}
