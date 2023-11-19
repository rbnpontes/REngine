using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("copy")]
	public class CopyRenderTargetNode : PostProcessNode
	{
		private readonly CopyRenderTargetFeature pFeature = new();
		public CopyRenderTargetNode() : base(nameof(CopyRenderTargetNode))
		{
		}

		protected override PostProcessFeature GetPostProcessFeature()
		{
			return pFeature;
		}
	}
}
