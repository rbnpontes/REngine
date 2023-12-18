using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox.BaseSample
{
	public interface ISample : IDisposable
	{
		public IWindow? Window { get; set; }
		public void Load(IServiceProvider provider);
		public void Update(IServiceProvider provider);
	}
}
