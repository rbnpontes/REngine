using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;

namespace REngine.RPI.Features.PostProcess
{
    public class GrayscalePostProcess(IAssetManager assetManager) : PostProcessFeature
    {
        protected override ShaderStream OnGetShaderCode()
        {
            return new StreamedShaderStream(assetManager.GetStream("Shaders/PostProcess/grayscale_ps.hlsl"));
        }
#if PROFILER
        protected override void OnExecute(ICommandBuffer command)
        {
            using(Profiler.Instance.Begin())
                base.OnExecute(command);
        }
#endif
    }
}
