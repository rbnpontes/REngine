using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Tests.DependencyInjection.Models
{
	internal class FooAndBarService
	{
		public IFooService Foo { get; private set; }
		public IBarService Bar { get; private set; }
		public FooAndBarService(IFooService foo, IBarService bar) 
		{
			Foo = foo;
			Bar = bar;
		}
	}
}
