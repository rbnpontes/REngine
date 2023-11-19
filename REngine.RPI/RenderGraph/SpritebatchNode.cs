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
	public class SpritebatchNode : GraphicsRenderFeatureNode
	{
		private static readonly ulong[] sExpectedWriteResources =
		{
			BackBufferSlotHash,
			DepthBufferSlotHash
		};

		private IRenderFeature? pFeature;
		public SpritebatchNode() : base(nameof(SpritebatchNode))
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pFeature ??= provider.Get<ISpriteBatch>().Feature;
			base.OnRun(provider);
		}

		protected override IRenderFeature GetFeature()
		{
			if (pFeature is null)
				throw new NullReferenceException("Render Feature is null. It seems SpriteBatch Render Feature has not been loaded.");
			return pFeature;
		}

		protected override IEnumerable<ulong> GetExpectedWriteResourceSlots()
		{
			return sExpectedWriteResources;
		}
	}
}
#endif
