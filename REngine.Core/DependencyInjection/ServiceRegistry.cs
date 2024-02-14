using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Reflection;

namespace REngine.Core.DependencyInjection
{
	public delegate Interface NoDepsActivationCall<Interface>();
	public delegate Interface ActivationCall<Interface>(object[] dependencies);
	public delegate Interface PostActivationCall<Interface>(IServiceProvider provider);
	public delegate void ModuleCall(IServiceRegistry registry);

	public interface IServiceRegistry
	{
		IServiceRegistry Add<Interface, Target>() where Target : class;
		IServiceRegistry Add<Target>() where Target : class;

		IServiceRegistry Add<Interface>(NoDepsActivationCall<Interface> call);
		IServiceRegistry Add<Interface>(ActivationCall<Interface> call, IEnumerable<Type> dependencies);
		IServiceRegistry Add<Interface>(PostActivationCall<Interface> call);

		IServiceProvider Build();
	}

	internal class ServiceRegistryImpl : IServiceRegistry
	{
		private readonly Dictionary<Type, ServiceConstructor> pConstructors = new ();
		private readonly Dictionary<Type, PostActivationCall<object>> pPostDependencies = new ();

		public IServiceRegistry Add<Interface, Target>() where Target: class
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Interface), typeof(Target));
			constructor.ActivationCall = deps =>
			{
				var result = ActivatorExtended.CreateInstance(typeof(Target), deps);
				if(result is null)
					throw new NullReferenceException($"Could not possible to create {typeof(Target).Name} type.");
				return result;
			};
			pConstructors[typeof(Interface)] = constructor;
			return this;
		}

		public IServiceRegistry Add<Target>() where Target : class
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Target));
			constructor.ActivationCall = deps =>
			{
				var result = ActivatorExtended.CreateInstance(typeof(Target), deps);
				if (result is null)
					throw new NullReferenceException($"Could not possible to create {typeof(Target).Name} type.");
				return result;
			};
			pConstructors[typeof(Target)] = constructor;
			return this;
		}

		public IServiceRegistry Add<Interface>(NoDepsActivationCall<Interface> call)
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Interface), ServiceConstructorType.Lambda);
			constructor.ActivationCall = (_) =>
			{
				var result = call();
				if (result is null)
					throw new NullReferenceException($"ActivationCall has retrieved null at service {typeof(Interface).Name}.");
				return result;
			};

			// dependencies must overwrite
			pConstructors[typeof(Interface)] = constructor;
			return this;
		}

		public IServiceRegistry Add<Interface>(ActivationCall<Interface> call, IEnumerable<Type> dependencies)
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Interface), ServiceConstructorType.Lambda);
			constructor.ActivationCall = (deps) =>
			{
				var result = call(deps);
				if (result is null)
					throw new NullReferenceException($"ActivationCall has retrieved null at service {typeof(Interface).Name}.");
				return result;
			};
			constructor.Dependencies = dependencies;

			pConstructors.Add(typeof(Interface), constructor);
			return this;
		}

		public IServiceRegistry Add<Interface>(PostActivationCall<Interface> call)
		{
			pPostDependencies.Add(typeof(Interface), (provider) =>
			{
				var result = call(provider);
				if (result is null)
					throw new NullReferenceException($"ActivationCall has retrieved null at service {typeof(Interface).Name}.");
				return result;
			});
			return this;
		}

		public IServiceProvider Build()
		{
			Dictionary<Type, object> services = new Dictionary<Type, object>();
			var provider = new ServiceProviderImpl(services);
			// insert service provider into dependency injection
			services.Add(typeof(IServiceProvider), provider);
			
			ServiceResolver resolver = new ServiceResolver(pConstructors);
			resolver.Resolve(services);

			// execute post dependencies. this dependencies requires IServiceProvider created
			var postDependencies = pPostDependencies.ToList();
			// post dependencies can insert new dependencies on registry
			// in this case, we must loop until no dependencies was added
			while (postDependencies.Count > 0)
			{
				pPostDependencies.Clear();
				foreach (var pair in postDependencies)
					services.Add(pair.Key, pair.Value(provider));

				postDependencies = pPostDependencies.ToList();
			}
			return provider;
		}
	}

	public static class ServiceRegistryFactory
	{
		public static IServiceRegistry Build()
		{
			return new ServiceRegistryImpl();
		}
	}
}
