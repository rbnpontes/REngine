using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.DependencyInjection
{
	internal class WrappedServiceProvider : IServiceProvider
	{
		private List<IServiceProvider> pProviders;
		public WrappedServiceProvider(IServiceProvider[] providers)
		{
			pProviders = providers.ToList();
		}

		public void AddProvider(IServiceProvider provider)
		{
			if (pProviders.Contains(provider))
				return;
			pProviders.Add(provider);
		}

		public object? GetService(Type serviceType)
		{
			object?[] objects = new object?[pProviders.Count];
			Parallel.For(0, pProviders.Count, i =>
			{
				objects[i] = pProviders[i].GetService(serviceType);
			});

			return objects.Where(x => x != null).FirstOrDefault();
		}
	}

	public static class DependencyInjectionExtensions
	{
		public static T Get<T>(this IServiceProvider curr)
		{
			T? obj = (T?)curr.GetService(typeof(T));

			if (obj is null)
				throw new NullReferenceException($"Not found service with this type {typeof(T).Name}.");
			return obj;
		}
		public static T? GetOrDefault<T>(this IServiceProvider curr)
		{
			return (T?)curr.GetService(typeof(T));
		}

		public static IServiceProvider Compose(this IServiceProvider curr, IServiceProvider provider, bool overwrite = false)
		{
			// if both providers inherits same type, then we merge internal registries
			if(curr is ServiceProviderImpl && provider is ServiceProviderImpl)
			{
				var first = (ServiceProviderImpl)curr;
				var second = (ServiceProviderImpl)provider;

				var currServices = first.GetServices();
				var services = second.GetServices();
				foreach(var pair in services)
				{
					if (currServices.ContainsKey(pair.Key) && !overwrite)
						continue;
					currServices.Add(pair.Key, pair.Value);
				}

				return curr;
			}

			if(curr is WrappedServiceProvider)
			{
				((WrappedServiceProvider)curr).AddProvider(provider);
				return curr;
			}
			else if(provider is WrappedServiceProvider)
			{
				((WrappedServiceProvider)provider).AddProvider(curr);
				return curr;
			}

			// If providers has been created by another dependency injection service
			// Then we must use WrapperServiceProvider to wrap both providers into one.
			return new WrappedServiceProvider(new IServiceProvider[]
			{
				curr,
				provider
			});
		}
	}
}
