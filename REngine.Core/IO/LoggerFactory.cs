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

	internal class NonGenericLoggerImpl : ILogger 
	{
		private ILoggerFactory pFactory;
		private string pTag;

		public NonGenericLoggerImpl(ILoggerFactory factory, Type type)
		{
			pFactory = factory;
			pTag = type.Name;
		}
		public NonGenericLoggerImpl(ILoggerFactory factory, string tagName, Type type)
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

		public ILogger EndProfile(string key)
		{
			throw new NotImplementedException();
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

		public ILogger Profile(string key)
		{
			throw new NotImplementedException();
		}

		public ILogger Success(params object[] args)
		{
			pFactory.Log(LogSeverity.Success, pTag, args);
			return this;
		}

		public ILogger Use(Type type)
		{
			return new NonGenericLoggerImpl(pFactory, pTag, type);
		}

		public ILogger Warning(params object[] args)
		{
			pFactory.Log(LogSeverity.Warning, pTag, args);
			return this;
		}
	}
	internal class LoggerImpl<T> : ILogger<T> 
	{
		private ILoggerFactory pFactory;
		private string pTag;

		public LoggerImpl(ILoggerFactory factory)
		{
			pFactory = factory;
			pTag = typeof(T).Name;
		}
		public LoggerImpl(ILoggerFactory factory, string tagName)
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

		public ILogger<T> EndProfile(string key)
		{
			throw new NotImplementedException();
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

		public ILogger<T> Profile(string key)
		{
			throw new NotImplementedException();
		}

		public ILogger<T> Success(params object[] args)
		{
			pFactory.Log(LogSeverity.Success, pTag, args);
			return this;
		}

		public ILogger<U> Use<U>()
		{
			return new LoggerImpl<U>(pFactory, pTag);
		}

		public ILogger<T> Warning(params object[] args)
		{
			pFactory.Log(LogSeverity.Warning, pTag, args);
			return this;
		}
	}

	public class DebugLoggerFactory : ILoggerFactory
	{
		private object pSync = new object();
		private LinkedList<StringBuilder> pLogQueue = new LinkedList<StringBuilder>();
		private Task? pLogTask;

		public ILogger<T> Build<T>()
		{
			return new LoggerImpl<T>(this);
		}
		public ILogger Build(Type type)
		{
			return new NonGenericLoggerImpl(this, type);
		}

		public ILoggerFactory Log(LogSeverity severity, string tag, object[] args)
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
			log.Append("]: ");

			for(int i = 0;i < args.Length; ++i)
			{
				log.Append(args[i].ToString());
				if (i < args.Length - 1)
					log.Append(" ");
			}

			lock (pSync)
			{
				pLogQueue.AddLast(log);
				if (pLogTask is null)
					StartLogTask();
			}

			return this;
		}

		private void StartLogTask()
		{
			pLogTask = Task.Run(TaskJob);
		}

		private void TaskJob()
		{
			bool repeat = false;
			do
			{
				LinkedList<StringBuilder> queue;

				lock (pSync)
				{
					queue = pLogQueue;
					pLogQueue = new LinkedList<StringBuilder>();
				}

				foreach (var log in queue)
					Debug.WriteLine(log.ToString());

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
	}
}
