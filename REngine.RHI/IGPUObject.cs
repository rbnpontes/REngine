using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public delegate void GPUObjectEvent(object sender, EventArgs args);

	public interface IGPUObject : IDisposable
	{
		public event GPUObjectEvent OnDispose;
		public string Name { get; }
	}
}
