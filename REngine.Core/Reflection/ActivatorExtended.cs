using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Reflection
{
	public static class ActivatorExtended
	{
		public static object CreateInstance(Type type, object[] args)
		{
			var argTypes = args.Select(x => x.GetType()).ToArray();
			var ctor = ConstructorUtils.FindSuitableConstructor(type, argTypes);
			
			if (ctor is null)
				throw new NullReferenceException($"Not found suitable constructor for given type '{type.Name}'");

			var paramArr = Expression.Parameter(typeof(object[]), "args");
			var parameters = new Expression[argTypes.Length];
			for (var i = 0; i < parameters.Length; ++i)
			{
				var binExp = Expression.ArrayIndex(paramArr, Expression.Constant(i));
				parameters[i] = Expression.Convert(binExp, argTypes[i]);
			}

			var ctorExp = Expression.New(ctor, parameters);
			var func = Expression
				.Lambda<Func<object[], object>>(ctorExp, paramArr)
				.Compile();

			return func(args);
		}
		public static T CreateInstance<T>(object[] args)
		{
			var instance = (T)CreateInstance(typeof(T), args);
			if (instance is null)
				throw new NullReferenceException(
					$"Could not possible to create instance of given type {typeof(T).Name}");
			return instance;
		}
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

			return CreateInstance(type, parameters ?? []);
		}
	}
}
