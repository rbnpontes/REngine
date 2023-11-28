using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;
using REngine.RPI.Features;
using REngine.RPI.Features.PostProcess;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.PostProcess
{
	[NodeTag("postprocess.blur")]
	public class BlurNode : PostProcessNode
	{
		private const string DirectionsPropKey = "directions";
		private const string QualityPropKey = "quality";
		private const string SizePropKey = "size";

		private static readonly ulong DirectionsHashKey = Hash.Digest(DirectionsPropKey);
		private static readonly ulong QualityHashKey = Hash.Digest(QualityPropKey);
		private static readonly ulong SizeHashKey = Hash.Digest(SizePropKey);

		private readonly BlurPostProcess pFeature = new();

		public BlurNode() : base(nameof(BlurNode))
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if(properties.TryGetValue(DirectionsHashKey, out var directions))
				pFeature.Directions = float.Parse(directions);
			if(properties.TryGetValue(QualityHashKey, out var quality))
				pFeature.Quality = float.Parse(quality);
			if(properties.TryGetValue(SizeHashKey, out var size))
				pFeature.Size = float.Parse(size);

			base.OnSetup(properties);
		}

		protected override PostProcessFeature GetPostProcessFeature()
		{
			return pFeature;
		}
	}
}
