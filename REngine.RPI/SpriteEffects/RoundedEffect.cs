using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;

namespace REngine.RPI.SpriteEffects
{
	public sealed class RoundedEffect : BasicSpriteEffect
	{
		public RoundedEffect() : base(nameof(RoundedEffect))
		{
			PixelShader = new FileShaderStream(Path.Join(EngineSettings.AssetsShadersPath, "rounded_sprite_effect_ps.hlsl"));
		}
	}
}
