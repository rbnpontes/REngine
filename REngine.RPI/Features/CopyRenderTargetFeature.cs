using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;

namespace REngine.RPI.Features
{
	public class CopyRenderTargetFeature : PostProcessFeature
	{
		protected override ShaderStream OnGetShaderCode()
		{
			return new FileShaderStream(
				Path.Join(EngineSettings.AssetsShadersPath, "copy_rt_ps.hlsl")
			);
		}
	}
}
