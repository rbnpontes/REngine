using REngine.Core.Mathematics;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public sealed class ComponentSerializerFactory
	{
		private readonly Dictionary<ulong, IComponentSerializer> pResolvers = new Dictionary<ulong, IComponentSerializer>();
		private readonly IServiceProvider pProvider;

		public ComponentSerializerFactory(IServiceProvider provider)
		{
			pProvider = provider;
		}

		public IComponentSerializer GetSerializer<T>() where T : Component
		{
			return GetSerializer(typeof(T));
		}

		public IComponentSerializer GetSerializer(Type type)
		{
#if DEBUG
			if (!type.IsAssignableTo(typeof(Component)))
				throw new Exception($"Type must implement '{nameof(Component)}'");
#endif
			var code = GetTypeCode(type);
			if (pResolvers.TryGetValue(code, out IComponentSerializer? resolver))
				return resolver;

			var attr = type.GetCustomAttribute<ComponentSerializerAttribute>();
			if (attr is null)
			{
				var defaultResolver = new DefaultComponentSerializer(pProvider);
				defaultResolver.SetComponentType(type);
				resolver = defaultResolver;
			}
			else
				resolver = CreateResolver(attr.ResolverType);

			pResolvers[code] = resolver;
			return resolver;
		}

		public IComponentSerializer? FindSerializer(ulong typeCode)
		{
			pResolvers.TryGetValue(typeCode, out IComponentSerializer? resolver);
			return resolver;
		}

		/// <summary>
		/// Iterate over all components and create all serializers
		/// This is an expensive operation, so becarefull when call this
		/// make sure this method is called while is loading something
		/// </summary>
		public ComponentSerializerFactory CollectSerializers()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach(var assembly in assemblies)
			{
				var types =assembly.GetTypes();
				foreach(var type in types)
				{
					if (type.IsAssignableTo(typeof(Component)) && type != typeof(Component))
						GetSerializer(type);
				}
			}

			return this;
		}

		public static ulong GetTypeCode(Type type)
		{
			return Hash.Digest(type.FullName ?? type.Name);
		}

		private IComponentSerializer CreateResolver(Type type)
		{
			var paramValues = Array.Empty<object>();
			var ctors = type.GetConstructors();
			var suitableCtor = ctors.Where(
				x =>
				{
					var parameters = x.GetParameters();
					paramValues = new object[parameters.Length];
					for (int i = 0; i < parameters.Length; ++i)
					{
						var param = pProvider.GetService(parameters[i].ParameterType);
						if (param is null)
							return false;
						paramValues[i] = param;
					}

					return true;
				}
			).FirstOrDefault();

			if (suitableCtor is null)
				throw new NullReferenceException($"Not found suitable constructor for this type resolver '{type.Name}'");

			var resolver = Activator.CreateInstance(type, paramValues) as IComponentSerializer;
			if (resolver is null)
				throw new NullReferenceException($"Error has ocurred while is creating '{type.Name}' instance.");
			return resolver;
		}
	}
}
