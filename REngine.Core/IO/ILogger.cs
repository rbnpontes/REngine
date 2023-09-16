using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public interface ILogger
	{
		/// <summary>
		/// Creates a nested logger
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public ILogger Use(Type type);
		public ILogger Info(params object[] args);
		public ILogger Debug(params object[] args);
		public ILogger Warning(params object[] args);
		public ILogger Success(params object[] args);
		public ILogger Error(params object[] args);
		public ILogger Critical(params object[] args);
		public ILogger Profile(string key);
		public ILogger EndProfile(string key);
	}
	public interface ILogger<T>
	{
		/// <summary>
		/// Create a nested logger 
		/// </summary>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public ILogger<U> Use<U>();
		public ILogger<T> Info(params object[] args);
		public ILogger<T> Debug(params object[] args);
		public ILogger<T> Warning(params object[] args);
		public ILogger<T> Success(params object[] args);
		public ILogger<T> Error(params object[] args);
		public ILogger<T> Critical(params object[] args);
		public ILogger<T> Profile(string key);
		public ILogger<T> EndProfile(string key);
	}
}
