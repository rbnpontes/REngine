using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if RENGINE_RENDERGRAPH
namespace REngine.RPI.RenderGraph
{
	[NodeTag("spritebatch-pass")]
	public class SpritebatchNode : RenderFeatureNode
	{
		private IRenderFeature? pFeature;
		public SpritebatchNode() : base(nameof(SpritebatchNode))
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if (pFeature is null)
				pFeature = provider.Get<ISpriteBatch>().Feature;
			base.OnRun(provider);
		}

		protected override IRenderFeature GetFeature()
		{
			if (pFeature is null)
				throw new NullReferenceException("Render Feature is null. It seems SpriteBatch Render Feature has not been loaded.");
			return pFeature;
		}
	}
}
#endif
