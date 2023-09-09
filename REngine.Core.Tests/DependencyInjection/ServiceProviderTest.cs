using Microsoft.Win32;
using REngine.Core.DependencyInjection;
using REngine.Core.Tests.DependencyInjection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Tests.DependencyInjection
{
	public class ServiceProviderTest
	{
		[Test]
		public void MustGet()
		{
			IServiceProvider provider = CreateProvider();

			Assert.IsNotNull(provider.GetService(typeof(FooService)));
			Assert.IsNotNull(provider.GetService(typeof(BarService)));
			Assert.IsNotNull(provider.GetService(typeof(IFooService)));
			Assert.IsNotNull(provider.GetService(typeof(IBarService)));
			Assert.IsNotNull(provider.GetService(typeof(FooAndBarService)));

			Assert.IsNotNull(provider.GetOrDefault<FooService>());
			Assert.IsNotNull(provider.GetOrDefault<BarService>());
			Assert.IsNotNull(provider.GetOrDefault<IFooService>());
			Assert.IsNotNull(provider.GetOrDefault<IBarService>());

			FooAndBarService? fooAndBar;
			Assert.IsNotNull(fooAndBar = provider.GetOrDefault<FooAndBarService>());
			Assert.IsNotNull(fooAndBar?.Foo);
			Assert.IsNotNull(fooAndBar?.Bar);

			Assert.Pass();
		}


		private IServiceProvider CreateProvider()
		{
			IServiceRegistry registry = ServiceRegistryFactory.Build();
			registry.Add<FooService>();
			registry.Add<IBarService, BarService>();
			registry.Add<IFooService>(() => new FooService());
			registry.Add(
				(deps) => new FooAndBarService((IFooService)deps[0], (IBarService)deps[1]),
				new Type[] { typeof(IFooService), typeof(IBarService) }
			);
			return registry.Build();
		}
	}
}
