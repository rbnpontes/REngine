using REngine.Core.DependencyInjection;
using REngine.Core.Tests.DependencyInjection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Tests.DependencyInjection
{
	public class ServiceRegistryTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void MustFactoryBuild()
		{
			Assert.IsNotNull(ServiceRegistryFactory.Build());
			Assert.Pass();
		}
		
		[Test]
		public void MustAdd()
		{
			IServiceRegistry registry = ServiceRegistryFactory.Build();
			registry.Add<FooService>();
			registry.Add<IBarService, BarService>();
			registry.Add<IFooService>(() => new FooService());
			registry.Add(
				(deps) =>new FooAndBarService((IFooService)deps[0], (IBarService)deps[1]), 
				new Type[] { typeof(IFooService), typeof(IBarService) }
			);

			Assert.Pass();
		}

		[Test]
		public void MustBuild()
		{
			IServiceRegistry registry = ServiceRegistryFactory.Build();
			registry.Add<FooService>();
			registry.Add<IBarService, BarService>();
			registry.Add<IFooService>(() => new FooService());
			registry.Add(
				(deps) => new FooAndBarService((IFooService)deps[0], (IBarService)deps[1]),
				new Type[] { typeof(IFooService), typeof(IBarService) }
			);

			IServiceProvider provider = registry.Build();

			Assert.IsNotNull(provider);
			Assert.Pass();
		}
	}
}
