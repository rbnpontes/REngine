using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	internal class ServiceProviderImpl(Dictionary<Type, object> services) : IServiceProvider
	{
		public object? GetService(Type serviceType)
		{
			services.TryGetValue(serviceType, out var result);
			return result;
		}

		public Dictionary<Type, object> GetServices()
		{
			return services;
		}
	}
}
