using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Features;
using REngine.RPI.Features.PostProcess;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.PostProcess
{
	[NodeTag("postprocess.crt")]
	public sealed class CrtPostProcessNode() : PostProcessNode(nameof(CrtPostProcessNode))
	{
		private readonly CrtPostProcess pFeature = new();

		protected override PostProcessFeature GetPostProcessFeature()
		{
			return pFeature;
		}
	}
}
