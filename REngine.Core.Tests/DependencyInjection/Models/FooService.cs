using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Tests.DependencyInjection.Models
{
	interface IFooService
	{
		public void Test();
	}
	internal class FooService : IFooService
	{
		public FooService() { }
		public void Test()
		{

		}
	}
}
