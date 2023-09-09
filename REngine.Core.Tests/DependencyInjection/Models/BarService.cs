using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Tests.DependencyInjection.Models
{
	interface IBarService
	{
		public void Test();
	}
	internal class BarService : IBarService
	{
		public BarService() { }
		public void Test()
		{
		}
	}
}
