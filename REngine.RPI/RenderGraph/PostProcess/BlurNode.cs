using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RPI.Features;
using REngine.RPI.Features.PostProcess;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph.PostProcess
{
	[NodeTag("postprocess.blur")]
	public sealed class BlurNode() : PostProcessNode(nameof(BlurNode))
	{
		private const string DirectionsPropKey = "directions";
		private const string QualityPropKey = "quality";
		private const string SizePropKey = "size";

		private static readonly ulong DirectionsHashKey = Hash.Digest(DirectionsPropKey);
		private static readonly ulong QualityHashKey = Hash.Digest(QualityPropKey);
		private static readonly ulong SizeHashKey = Hash.Digest(SizePropKey);
		
		private BlurPostProcess? pFeature;
		private float pDirections = BlurPostProcess.DefaultDirections;
		private float pQuality = BlurPostProcess.DefaultQuality;
		private float pSize = BlurPostProcess.DefaultSize;
		
		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if(properties.TryGetValue(DirectionsHashKey, out var directions))
				pDirections = float.Parse(directions);
			if(properties.TryGetValue(QualityHashKey, out var quality))
				pQuality = float.Parse(quality);
			if(properties.TryGetValue(SizeHashKey, out var size))
				pSize = float.Parse(size);

			base.OnSetup(properties);
		}

		protected override PostProcessFeature GetPostProcessFeature()
		{
			pFeature ??= new BlurPostProcess(ServiceProvider.Get<IAssetManager>());
			pFeature.Directions = pDirections;
			pFeature.Quality = pQuality;
			pFeature.Size = pSize;
			return pFeature;
		}
	}
}
