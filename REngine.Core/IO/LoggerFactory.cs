using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public enum LogSeverity
	{
		Info,
		Debug,
		Warning,
		Success,
		Error,
		Critical
	}

	public interface ILoggerFactory
	{
		public ILogger<T> Build<T>();
		public ILogger Build(Type genericType);
		public ILoggerFactory Log(LogSeverity severity, string tag, object[] args);
	}

	public sealed class NonGenericLogger : ILogger
	{
		private readonly ILoggerFactory pFactory;
		private readonly string pTag;

		public NonGenericLogger(ILoggerFactory factory, Type type)
		{
			pFactory = factory;
			pTag = type.Name;
		}
		public NonGenericLogger(ILoggerFactory factory, string tagName, Type type)
		{
			pFactory = factory;
			pTag = tagName + "::" + type.Name;
		}

		public ILogger Critical(params object[] args)
		{
			pFactory.Log(LogSeverity.Critical, pTag, args);
			return this;
		}

		public ILogger Debug(params object[] args)
		{
			pFactory.Log(LogSeverity.Debug, pTag, args);
			return this;
		}

		public ILogger Error(params object[] args)
		{
			pFactory.Log(LogSeverity.Error, pTag, args);
			return this;
		}

		public ILogger Info(params object[] args)
		{
			pFactory.Log(LogSeverity.Info, pTag, args);
			return this;
		}

		private static int GetProfileKey(string baseKey, string tag)
		{
			return string.Intern($"{tag}:{baseKey}").GetHashCode();
		}

		public ILogger Profile(string key)
		{
			PerformanceMeasure.BeginMeasure(GetProfileKey(key, pTag));
			return this;
		}

		public ILogger EndProfile(string key)
		{
			if (!PerformanceMeasure.EndMeasure(GetProfileKey(key, pTag), out var elapsed))
				return this;
			pFactory.Log(LogSeverity.Info, $"{pTag}:Profile#{elapsed}", [key]);
			return this;
		}

		public ILogger Success(params object[] args)
		{
			pFactory.Log(LogSeverity.Success, pTag, args);
			return this;
		}

		public ILogger Use(Type type)
		{
			return new NonGenericLogger(pFactory, pTag, type);
		}

		public ILogger Warning(params object[] args)
		{
			pFactory.Log(LogSeverity.Warning, pTag, args);
			return this;
		}
	}
	public sealed class Logger<T> : ILogger<T>
	{
		private readonly ILoggerFactory pFactory;

		private string pTag;
		private IDisposable? pProfilerScope;

		public Logger(ILoggerFactory factory)
		{
			pFactory = factory;
			pTag = typeof(T).Name;
		}
		public Logger(ILoggerFactory factory, string tagName)
		{
			pFactory = factory;
			pTag = tagName + "::" + typeof(T).Name;
		}

		public ILogger<T> Critical(params object[] args)
		{
			pFactory.Log(LogSeverity.Critical, pTag, args);
			return this;
		}

		public ILogger<T> Debug(params object[] args)
		{
			pFactory.Log(LogSeverity.Debug, pTag, args);
			return this;
		}

		public ILogger<T> Error(params object[] args)
		{
			pFactory.Log(LogSeverity.Error, pTag, args);
			return this;
		}

		public ILogger<T> Info(params object[] args)
		{
			pFactory.Log(LogSeverity.Info, pTag, args);
			return this;
		}

		private static int GetProfileKey(string baseKey, string tag)
		{
			return string.Intern($"{tag}:{baseKey}").GetHashCode();
		}
		public ILogger<T> Profile(string key)
		{
			PerformanceMeasure.BeginMeasure(GetProfileKey(key, pTag));
			return this;
		}

		public ILogger<T> EndProfile(string key)
		{
			var hashCode = GetProfileKey(key, pTag).GetHashCode();
			if (!PerformanceMeasure.EndMeasure(hashCode, out var elapsed))
				return this;
			
			pFactory.Log(LogSeverity.Info, $"{pTag}:Profile#{elapsed}", [key]);
			return this;
		}

		public ILogger<T> Success(params object[] args)
		{
			pFactory.Log(LogSeverity.Success, pTag, args);
			return this;
		}

		public ILogger<U> Use<U>()
		{
			return new Logger<U>(pFactory, pTag);
		}

		public ILogger<T> Warning(params object[] args)
		{
			pFactory.Log(LogSeverity.Warning, pTag, args);
			return this;
		}
	}

	public abstract class BaseLoggerFactory
	{
		protected string BuildLog(LogSeverity severity, string tag, object[] args)
		{
			StringBuilder log = new StringBuilder();
			switch (severity)
			{
				case LogSeverity.Info:
					log.Append("[Info]");
					break;
				case LogSeverity.Warning:
					log.Append("[Warning]");
					break;
				case LogSeverity.Error:
					log.Append("[Error]");
					break;
				case LogSeverity.Success:
					log.Append("[Success]");
					break;
				case LogSeverity.Debug:
					log.Append("[Debug]");
					break;
				case LogSeverity.Critical:
					log.Append("[Critical]");
					break;
			}
			log.Append("[");
			log.Append(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss:f"));
			log.Append("]");
			log.Append("[");
			log.Append(tag);
			log.Append("]: ");

			for (int i = 0; i < args.Length; ++i)
			{
				log.Append(args[i].ToString());
				if (i < args.Length - 1)
					log.Append(" ");
			}

			return log.ToString();
		}
	}
	public abstract class AsyncLoggerFactory : BaseLoggerFactory
	{
		private readonly object pSync = new();

		private Queue<(LogSeverity, string)> pLogQueue = new();
		private Task? pLogTask;

		protected void EnqueueLog(LogSeverity severity, string log)
		{
			lock (pSync)
			{
				pLogQueue.Enqueue((severity, log));
				if (pLogTask is null)
					StartLogTask();
			}
		}

		private void StartLogTask()
		{
			pLogTask = Task.Run(TaskJob);
		}

		private void TaskJob()
		{
			bool repeat;
			do
			{
				Queue<(LogSeverity, string)> queue;

				lock (pSync)
				{
					queue = pLogQueue;
					pLogQueue = new Queue<(LogSeverity, string)>();
				}

				OnAsyncBeforeExecute();
				while(queue.TryDequeue(out var log))
					OnAsyncExecuteLog(log.Item1, log.Item2);
				OnAsyncAfterExecute();

				// if has new log items to be processed, we must repeat loop again
				// otherwise, unset log task and finish job.
				lock (pSync)
				{
					if (!(repeat = pLogQueue.Count > 0))
						pLogTask = null;
				}
			}
			while (repeat);
		}

		protected virtual void OnAsyncBeforeExecute() { }
		protected virtual void OnAsyncAfterExecute() { }

		protected abstract void OnAsyncExecuteLog(LogSeverity severity, string log);
	}
	public abstract class DefaultLoggerFactory : AsyncLoggerFactory, ILoggerFactory
	{
		public ILogger<T> Build<T>()
		{
			return new Logger<T>(this);
		}
		public ILogger Build(Type type)
		{
			return new NonGenericLogger(this, type);
		}

		public ILoggerFactory Log(LogSeverity severity, string tag, object[] args)
		{
			EnqueueLog(severity, BuildLog(severity, tag, args));
			return this;
		}
	}

	public class DebugLoggerFactory : DefaultLoggerFactory, ILoggerFactory
	{
		protected override void OnAsyncExecuteLog(LogSeverity severity, string log)
		{
			Debug.WriteLine(log);

			ConsoleColor currColor = Console.ForegroundColor;
			switch (severity)
			{
				case LogSeverity.Error:
				case LogSeverity.Critical:
					Console.ForegroundColor = ConsoleColor.DarkRed;
					break;
				case LogSeverity.Warning:
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;
				case LogSeverity.Debug:
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;
				case LogSeverity.Info:
					Console.ForegroundColor = currColor;
					break;
				case LogSeverity.Success:
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
			}
			Console.WriteLine(log);
			Console.ForegroundColor = currColor;
		}
	}

	public class FileLoggerFactory : DefaultLoggerFactory, ILoggerFactory
	{
		protected readonly string pLogPath;

		private FileStream? pFileStream;
		private TextWriter? pWriter;

		public FileLoggerFactory(string logPath)
		{
			pLogPath = logPath;
		}

		protected override void OnAsyncBeforeExecute()
		{
			if(!File.Exists(pLogPath))
				File.Create(pLogPath).Dispose();
			pWriter = new StreamWriter(pLogPath, true);
		}
		protected override void OnAsyncExecuteLog(LogSeverity severity, string log)
		{
			pWriter?.WriteLine(log);
		}
		protected override void OnAsyncAfterExecute()
		{
			pWriter?.Dispose();
			pFileStream?.Dispose();
		}
	}

	public class ComposedLoggerFactory : ILoggerFactory
	{
		class ComposedLogger<T> : ILogger<T> 
		{
			private IEnumerable<ILogger<T>> pLoggers;
			public ComposedLogger(IEnumerable<ILogger<T>> loggers)
			{
				pLoggers = loggers;
			}

			public ILogger<T> Critical(params object[] args)
			{
				foreach(var logger in pLoggers)
					logger.Critical(args);
				return this;
			}

			public ILogger<T> Debug(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Debug(args);
				return this;
			}

			public ILogger<T> EndProfile(string key)
			{
				foreach (var logger in pLoggers)
					logger.EndProfile(key);
				return this;
			}

			public ILogger<T> Error(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Error(args);
				return this;
			}

			public ILogger<T> Info(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Info(args);
				return this;
			}

			public ILogger<T> Profile(string key)
			{
				foreach (var logger in pLoggers)
					logger.Profile(key);
				return this;
			}

			public ILogger<T> Success(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Success(args);
				return this;
			}

			public ILogger<U> Use<U>()
			{
				return new ComposedLogger<U>(pLoggers.Select(x => x.Use<U>()));
			}

			public ILogger<T> Warning(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Warning(args);
				return this;
			}
		}
		class ComposedNonGenericLogger : ILogger
		{
			private IEnumerable<ILogger> pLoggers;
			public ComposedNonGenericLogger(IEnumerable<ILogger> loggers)
			{
				pLoggers = loggers;
			}

			public ILogger Critical(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Critical(args);
				return this;
			}

			public ILogger Debug(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Debug(args);
				return this;
			}

			public ILogger EndProfile(string key)
			{
				foreach (var logger in pLoggers)
					logger.EndProfile(key);
				return this;
			}

			public ILogger Error(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Error(args);
				return this;
			}

			public ILogger Info(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Info(args);
				return this;
			}

			public ILogger Profile(string key)
			{
				foreach (var logger in pLoggers)
					logger.Profile(key);
				return this;
			}

			public ILogger Success(params object[] args)
			{
				foreach (var logger in pLoggers)
					logger.Success(args);
				return this;
			}

			public ILogger Use(Type type)
			{
				return new ComposedNonGenericLogger(pLoggers.Select(x => x.Use(type)));
			}

			public ILogger Warning(params object[] args)
			{
				foreach(var logger in pLoggers)
					logger.Warning(args);
				return this;
			}
		}

		private readonly IEnumerable<ILoggerFactory> pFactories;

		public ComposedLoggerFactory(IEnumerable<ILoggerFactory> factories) 
		{
			pFactories = factories;
		}

		public ILogger<T> Build<T>()
		{
			return new ComposedLogger<T>(
				pFactories.Select(x => x.Build<T>())
			);
		}

		public ILogger Build(Type genericType)
		{
			return new ComposedNonGenericLogger(
				pFactories.Select(x => x.Build(genericType))
			);
		}

		public ILoggerFactory Log(LogSeverity severity, string tag, object[] args)
		{
			foreach(var factory in pFactories)
				factory.Log(severity, tag, args);
			return this;
		}
	}
}
