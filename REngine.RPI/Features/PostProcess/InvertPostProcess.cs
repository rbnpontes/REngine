using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Resources;

namespace REngine.RPI.Features.PostProcess
{
	public sealed class InvertPostProcess(IAssetManager assetManager) : PostProcessFeature
	{
		protected override ShaderStream OnGetShaderCode()
		{
			return new StreamedShaderStream(assetManager.GetStream("Shaders/PostProcess/invertcolors_ps.hlsl"));
		}
	}
}
