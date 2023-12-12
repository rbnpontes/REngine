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
	[NodeTag("postprocess.sepia")]
	public sealed class SepiaPostProcessNode() : PostProcessNode(nameof(SepiaPostProcessNode))
	{
		private SepiaPostProcess? pFeature;
		protected override PostProcessFeature GetPostProcessFeature()
		{
			pFeature ??= new SepiaPostProcess(ServiceProvider.Get<IAssetManager>());
			return pFeature;
		}
	}
}
