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
	[NodeTag("postprocess.crt")]
	public sealed class CrtPostProcessNode() : PostProcessNode(nameof(CrtPostProcessNode))
	{
		private CrtPostProcess? pFeature;

		protected override PostProcessFeature GetPostProcessFeature()
		{
			pFeature ??= new CrtPostProcess(ServiceProvider.Get<IAssetManager>());
			return pFeature;
		}
	}
}
