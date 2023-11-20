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
	[NodeTag("postprocess.invert")]
	public sealed class InvertPostProcessNode : PostProcessNode
	{
		private readonly InvertPostProcess pFeature = new();
		public InvertPostProcessNode() : base(nameof(InvertPostProcessNode))
		{
		}

		protected override PostProcessFeature GetPostProcessFeature()
		{
			return pFeature;
		}
	}
}
