using REngine.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public class ComponentRegistryException : Exception
	{
		public ComponentRegistryException(string message) : base(message) { }
	}
	public sealed class ComponentsRegistry
	{
		private Dictionary<Type, Type> pRegistry = new();

		public ComponentsRegistry() { }

		public ComponentsRegistry Register<Interface, Target>()
		{
			return Register(typeof(Interface), typeof(Target));
		}
		public ComponentsRegistry Register<Target>()
		{
			return Register(typeof(Target), typeof(Target));
		}
		public ComponentsRegistry Register(Type interfaceType, Type targetType)
		{
			if (!interfaceType.IsAssignableFrom(targetType))
				throw new InvalidCastException("Target type is not assigned to interface type");
			if (interfaceType.IsAssignableFrom(targetType))
				throw new InvalidCastException("Target must be assignable to interface type");
			if (interfaceType.IsAssignableTo(typeof(ISceneComponent)))
				throw new ArgumentException($"Interface type must inherit {nameof(ISceneComponent)}");
			if (targetType.IsAssignableTo(typeof(ISceneComponent)))
				throw new ArgumentException($"Target type must implement {nameof(ISceneComponent)}");
			pRegistry[interfaceType] = targetType;
			return this;
		}

		public ISceneComponent Create<T>(IServiceProvider provider) where T : ISceneComponent 
		{ 
			return Create(typeof(T), provider);
		}
		public ISceneComponent Create(Type componentType, IServiceProvider provider)
		{
			if (!pRegistry.TryGetValue(componentType, out var targetType))
				throw new ComponentRegistryException($"Can´t create component '{componentType.Name}'. Registered type has not found");

			var ctors = targetType.GetConstructors().FirstOrDefault();
			if (ctors is null)
				throw new NullReferenceException($"Not found suitable constructor on type '{targetType.Name}'");

			var instance = Activator.CreateInstance(
				targetType,
				ctors
				.GetParameters()
				.Select(param=>
				{
					var obj = provider.GetService(param.ParameterType);
					if (obj is null)
						throw new ComponentRegistryException($"Not found injectable object for type '{param.ParameterType.Name}' while is creating component.");
					return obj;
				})
			) as ISceneComponent;
			if (instance is null)
				throw new ComponentRegistryException("Could not possible to create component. Create Instance has returned null");
			return instance;
		}
	}
}
