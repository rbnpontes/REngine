using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public interface IRenderFeature : IDisposable
	{
		public bool IsDirty { get; }
		public bool IsDisposed { get; }

		public IRenderFeature MarkAsDirty();
		public IRenderFeature Setup(IGraphicsDriver driver, IRenderer renderer);
		public IRenderFeature Compile(ICommandBuffer command);
		public IRenderFeature Execute(ICommandBuffer command);
	}
}
