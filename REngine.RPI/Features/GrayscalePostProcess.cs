using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;

namespace REngine.RPI.Features
{
	public class GrayscalePostProcess : PostProcessFeature
	{
		private readonly ShaderStream pShaderCode = new FileShaderStream(
			Path.Join(EngineSettings.AssetsShadersPath, "grayscale_ps.hlsl")
		);

		protected override void OnDispose()
		{
			base.OnDispose();
			pShaderCode.Dispose();
		}

		protected override ShaderStream OnGetShaderCode()
		{
			return pShaderCode;
		}
	}
}
