using REngine.RHI;
using REngine.RPI.RenderGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;

namespace REngine.RPI.Features
{
	public sealed class RenderGraphFeature : IRenderFeature
	{
		private readonly RenderGraphEntry pEntry;
		private readonly IRenderGraph pRenderGraph;

		public bool IsDirty => false;

		public bool IsDisposed { get; private set; }

		public RenderGraphFeature(IRenderGraph renderGraph, RenderGraphEntry entry)
		{
			pRenderGraph = renderGraph;
			pEntry = entry;
		}

		public IRenderFeature Compile(ICommandBuffer command)
		{
			return this;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			pEntry.Root.Dispose();
			pRenderGraph.RootEntry = null;
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public IRenderFeature Execute(ICommandBuffer command)
		{
#if PROFILER
			using var _ = Profiler.Instance.Begin();
#endif
			pRenderGraph.RootEntry = pEntry;
			pRenderGraph.Execute();
			return this;
		}

		public IRenderFeature MarkAsDirty()
		{
			return this;
		}

		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo)
		{
			return this;
		}
	}
}
