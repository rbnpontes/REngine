using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if RENGINE_IMGUI
namespace REngine.RPI
{
	public interface IImGuiSystem
	{
		public IGraphicsRenderFeature Feature { get; }
		public event EventHandler? OnGui;
	}
}
#endif