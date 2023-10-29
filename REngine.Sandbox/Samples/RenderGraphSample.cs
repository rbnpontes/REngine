using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RPI;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox.Samples
{
	[Sample("Render Graph")]
	internal class RenderGraphSample : ISample
	{
		private IVariableManager? pVarMgr;
		private IRenderGraph? pRenderGraph;
		private IRenderFeature? pFeature;
		private IRenderer? pRenderer;
		private IImGuiSystem? pImGuiSystem;

		public IWindow? Window { get; set; }

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pFeature);
			pFeature?.Dispose();

			if(pImGuiSystem != null)
				pRenderer?.AddFeature(pImGuiSystem.Feature);
		}

		public void Load(IServiceProvider provider)
		{
			pRenderer = provider.Get<IRenderer>();
			pRenderGraph = provider.Get<IRenderGraph>();
			pVarMgr = provider.Get<IVariableManager>();
			pImGuiSystem = provider.Get<IImGuiSystem>();

			var rootEntry = pRenderGraph.LoadFromFile(
				Path.Join(
					AppDomain.CurrentDomain.BaseDirectory,
					"Assets/default-rendergraph.xml"
				)
			);

			pFeature = new RenderGraphFeature(pRenderGraph, rootEntry);
			pRenderer.AddFeature(pFeature);
			// Remove ImGui feature, otherwise we will deal with double rendering
			pRenderer.RemoveFeature(
				pImGuiSystem.Feature
			);
		}

		public void Update(IServiceProvider provider)
		{
		}
	}
}
