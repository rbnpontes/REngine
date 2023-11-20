using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Reflection
{
	public static class ActivatorExtended
	{
		/// <summary>
		/// Creates an Instance of a given Generic Type dynamically
		/// Arguments will be resolved by IServiceProvider
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static T? CreateInstance<T>(IServiceProvider provider)
		{
			return (T)CreateInstance(provider, typeof(T))!;
		}

		/// <summary>
		/// Creates an Instance of a given Type dynamically
		/// Arguments will be resolved by IServiceProvider
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object? CreateInstance(IServiceProvider provider, Type type)
		{
			var ctor = type.GetConstructors().FirstOrDefault() 
			           ?? throw new Exception($"Can´t create an instance of '{type.FullName ?? type.Namespace}'.");
			var parameters = ctor
				.GetParameters()
				.Select(x => provider.GetService(x.ParameterType))
				.ToArray();

			return Activator.CreateInstance(type, parameters);
		}
	}
}
