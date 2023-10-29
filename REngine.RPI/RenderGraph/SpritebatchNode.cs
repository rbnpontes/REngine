using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("spritebatch-pass")]
	public class SpritebatchNode : RenderFeatureNode
	{
		public SpritebatchNode(ISpriteBatch spriteBatch) : base(spriteBatch.Feature, nameof(SpritebatchNode))
		{
		}
	}
}
