using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("imgui-pass")]
	public class ImGuiNode : RenderFeatureNode
	{
		public ImGuiNode(IImGuiSystem system) : base(system.Feature, nameof(ImGuiNode))
		{
		}
		public ImGuiNode(IRenderFeature feature) : base(feature, nameof(ImGuiNode)) { }
	}
}
