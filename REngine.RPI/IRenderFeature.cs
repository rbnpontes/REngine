using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public struct RenderFeatureSetupInfo
	{
		public IGraphicsDriver Driver;
		public IRenderer Renderer;
		public IBufferProvider BufferProvider;

		public RenderFeatureSetupInfo(IGraphicsDriver driver, IRenderer renderer, IBufferProvider provider)
		{
			Driver = driver;
			Renderer = renderer;
			BufferProvider = provider;
		}
	}

	public interface IRenderFeature : IDisposable
	{
		public bool IsDirty { get; }
		public bool IsDisposed { get; }

		public IRenderFeature MarkAsDirty();
		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo);
		public IRenderFeature Compile(ICommandBuffer command);
		public IRenderFeature Execute(ICommandBuffer command);
	}
}
