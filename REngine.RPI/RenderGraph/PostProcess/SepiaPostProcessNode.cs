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
	[NodeTag("postprocess.sepia")]
	public class SepiaPostProcessNode : PostProcessNode
	{
		private readonly SepiaPostProcess pFeature = new();
		public SepiaPostProcessNode() : base(nameof(SepiaPostProcessNode))
		{
		}

		protected override PostProcessFeature GetPostProcessFeature()
		{
			return pFeature;
		}
	}
}
