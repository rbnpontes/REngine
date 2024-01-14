using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Resources;

namespace REngine.RPI.Effects
{
	public sealed class RoundedEffect(
		IServiceProvider provider
	) : SpriteEffect(
		provider.Get<IAssetManager>(),
		provider.Get<IPipelineStateManager>(),
		provider.Get<GraphicsSettings>(),
		provider.Get<IShaderResourceBindingCache>(),
		provider.Get<IBufferManager>(),
		provider.Get<IShaderManager>()
	)
	{
		private readonly IAssetManager pAssetManager = provider.Get<IAssetManager>();
		protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
		{
			base.OnGetShaderCreateInfo(shaderType, out shaderCi);
			if (shaderType != ShaderType.Pixel) 
				return;
			FillRoundedShaderCode(pAssetManager, ref shaderCi);
		}

		public static void FillRoundedShaderCode(IAssetManager assetManager, ref ShaderCreateInfo shaderCi)
		{
			var shaderAsset = assetManager.GetAsset<ShaderAsset>("Shaders/rounded_sprite_effect_ps.hlsl");
			shaderCi.Name = $"[Pixel]{nameof(RoundedEffect)}";
			shaderCi.SourceCode = shaderAsset.ShaderCode;
		}
	}

	public sealed class RoundedTextureEffect(
		IServiceProvider provider
	) : TextureSpriteEffect(
		provider.Get<IAssetManager>(),
		provider.Get<IPipelineStateManager>(),
		provider.Get<GraphicsSettings>(),
		provider.Get<IShaderResourceBindingCache>(),
		provider.Get<IBufferManager>(),
		provider.Get<IShaderManager>()
	)
	{
		private readonly IAssetManager pAssetManager = provider.Get<IAssetManager>();
		protected override void OnGetShaderCreateInfo(ShaderType shaderType, out ShaderCreateInfo shaderCi)
		{
			base.OnGetShaderCreateInfo(shaderType, out shaderCi);
			if (shaderType != ShaderType.Pixel) 
				return;
			RoundedEffect.FillRoundedShaderCode(pAssetManager, ref shaderCi);
		}
	}
}
