using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	public class LazyServiceProvider : IServiceProvider
	{
		private Dictionary<Type, object> pInstances = new Dictionary<Type, object>();

		public object? GetService(Type serviceType)
		{
			object? result;
			pInstances.TryGetValue(serviceType, out result);
			return result;
		}

		public void AddService(Type type, object instance)
		{
			pInstances.Add(type, instance);
		}
		public void AddService<T>(T instance)
		{
			if (instance is null)
				return;
			AddService(typeof(T), instance);
		}
	}
}
