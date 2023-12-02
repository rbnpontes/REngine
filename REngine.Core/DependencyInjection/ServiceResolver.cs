using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	public class ServiceResolverException : Exception 
	{ 
		public ServiceResolverException(string message) : base(message) { }
	}
	
	internal class ServiceResolver(Dictionary<Type, ServiceConstructor> ctors)
	{
		public void Resolve(Dictionary<Type, object> instances)
		{
			foreach(var pair in ctors)
				ResolveRegistered(instances, pair.Value);
		}

		private object ResolveDependency(Dictionary<Type, object> instances, Type dependency)
		{
			// check first if dependency has been already resolved
			if (instances.TryGetValue(dependency, out var result))
				return result;

			// if dependency has not yet resolved, then search by registered dependencies
			if (ctors.TryGetValue(dependency, out var depCtor))
				return ResolveRegistered(instances, depCtor);

			throw new ServiceResolverException($"Dependency {dependency.Name} has not yet registered.");
		}
		private object ResolveRegistered(Dictionary<Type, object> instances, ServiceConstructor ctor)
		{
			if (ctor.ActivationCall == null)
				throw new NullReferenceException("ActivationCall is null.");

			if (instances.TryGetValue(ctor.InterfaceType, out var obj))
				return obj;

			object[] dependencies;
			if(ctor.ConstructorType == ServiceConstructorType.Reflection)
			{
				var ctorInfo = FindSuitableConstructor(instances, ctor.TargetType.GetConstructors());
				dependencies = ctorInfo
					.GetParameters()
					.Select(dep => ResolveDependency(instances, dep.ParameterType))
					.ToArray();

			}
			else
			{
				dependencies = ctor.Dependencies
					.Select(dep => ResolveDependency(instances, dep))
					.ToArray();
			}

			obj = ctor.ActivationCall(dependencies);
			instances[ctor.InterfaceType] = obj;
			if(ctor.TargetType != ctor.InterfaceType)
				instances[ctor.TargetType] = obj;
			return obj;
		}

		// TODO: complete unregistered resolve
		//private object ResolveUnregistered(Dictionary<Type, object> instances, Type type)
		//{
		//	object? result;
		//	if (instances.TryGetValue(type, out result))
		//		return result;

		//	var ctors = type.GetConstructors();
		//	if (ctors.Where(x => x.GetGenericArguments().Length == 0).Count() > 0)
		//	{
		//		result = Activator.CreateInstance(type);
		//		if (result is null)
		//			throw new NullReferenceException($"Error at instantation of unregistered type {type.Name}.");
		//		return result;
		//	}


		//	return result;
		//}

		private ConstructorInfo FindSuitableConstructor(Dictionary<Type, object> instances, ConstructorInfo[] ctors1)
		{
			var ctor = ctors1.Where(ctor =>
			{
				var args = ctor.GetParameters();
				foreach (var arg in args)
				{
					if (instances.ContainsKey(arg.ParameterType))
						continue;
					if (!ctors.ContainsKey(arg.ParameterType))
						continue;
				}

				return true;
			}).FirstOrDefault();

			if (ctor is null)
				throw new ServiceResolverException("Not found suitable constructor.");
			return ctor;
		}
	}
}
