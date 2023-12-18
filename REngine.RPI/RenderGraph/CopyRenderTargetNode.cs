using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("copy")]
	public class CopyRenderTargetNode() : PostProcessNode(nameof(CopyRenderTargetNode))
	{
		private CopyRenderTargetFeature? pFeature;
		protected override PostProcessFeature GetPostProcessFeature()
		{
			pFeature ??= new CopyRenderTargetFeature(ServiceProvider.Get<IAssetManager>());
			return pFeature;
		}
	}
}
