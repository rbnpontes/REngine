using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public interface IImGuiSystem
	{
		public IRenderFeature Feature { get; }
		public event EventHandler? OnGui;
	}
}
