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
    [NodeTag("postprocess.grayscale")]
    public class GrayScalePostProcessNode : PostProcessNode
    {
        private readonly GrayscalePostProcess pFeature = new();
        public GrayScalePostProcessNode() : base(nameof(GrayScalePostProcessNode))
        {
        }

        protected override PostProcessFeature GetPostProcessFeature()
        {
            return pFeature;
        }
    }
}
