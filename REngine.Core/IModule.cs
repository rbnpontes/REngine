using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;

namespace REngine.Core
{
	public interface IModule
	{
		public void Setup(IServiceRegistry registry);
	}
}
