using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RPI.Features;
using REngine.RPI.Features.PostProcess;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.PostProcess
{
	[NodeTag("postprocess.invert")]
	public sealed class InvertPostProcessNode() : PostProcessNode(nameof(InvertPostProcessNode))
	{
		private InvertPostProcess? pFeature;
		protected override PostProcessFeature GetPostProcessFeature()
		{
			pFeature ??= new InvertPostProcess(ServiceProvider.Get<IAssetManager>());
			return pFeature;
		}
	}
}
