﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		private Dictionary<Type, ServiceConstructor> pConstructors = new Dictionary<Type, ServiceConstructor>();
		private Dictionary<Type, PostActivationCall<object>> pPostDependencies = new Dictionary<Type, PostActivationCall<object>>();

		public IServiceRegistry Add<Interface, Target>() where Target: class
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Interface), typeof(Target));
			constructor.ActivationCall = deps =>
			{
				var result = Activator.CreateInstance(typeof(Target), deps);
				if(result is null)
					throw new NullReferenceException($"Could not possible to create {typeof(Target).Name} type.");
				return result;
			};
			pConstructors.Add(typeof(Interface), constructor);
			return this;
		}

		public IServiceRegistry Add<Target>() where Target : class
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Target));
			constructor.ActivationCall = deps =>
			{
				var result = Activator.CreateInstance(typeof(Target), deps);
				if (result is null)
					throw new NullReferenceException($"Could not possible to create {typeof(Target).Name} type.");
				return result;
			};
			pConstructors.Add(typeof(Target), constructor);
			return this;
		}

		public IServiceRegistry Add<Interface>(NoDepsActivationCall<Interface> call)
		{
			ServiceConstructor constructor = new ServiceConstructor(typeof(Interface), ServiceConstructorType.Lambda);
			constructor.ActivationCall = (_) =>
			{
				var result = call();
				if (result is null)
					throw new NullReferenceException($"ActivationCall has retrivied null at service {typeof(Interface).Name}.");
				return result;
			};

			pConstructors.Add(typeof(Interface), constructor);
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
			foreach(var pair in pPostDependencies)
				services.Add(pair.Key, pair.Value(provider));
			
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