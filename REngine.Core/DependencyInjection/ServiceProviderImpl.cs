using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	internal class ServiceProviderImpl : IServiceProvider
	{
		private Dictionary<Type, object> pServices;

		public ServiceProviderImpl(Dictionary<Type, object> services)
		{
			pServices = services;
		}

		public object? GetService(Type serviceType)
		{
			object? result;
			pServices.TryGetValue(serviceType, out result);
			return result;
		}

		public Dictionary<Type, object> GetServices()
		{
			return pServices;
		}
	}
}
