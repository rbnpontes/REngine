using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;

namespace REngine.RPI.Features.PostProcess
{
	public sealed class BlurPostProcess(IAssetManager assetManager) : PostProcessFeature
	{
		private IBuffer? pCBuffer;
		public const float DefaultDirections = 16;
		public const float DefaultQuality = 3;
		public const float DefaultSize = 8;
		public float Directions { get; set; } = DefaultDirections;
		public float Quality { get; set; } = DefaultQuality;
		public float Size { get; set; } = DefaultSize;
		protected override ShaderStream OnGetShaderCode()
		{
			return new StreamedShaderStream(assetManager.GetStream("Shaders/PostProcess/blur.hlsl"));
		}

		protected override void OnExecute(ICommandBuffer command)
		{
#if PROFILER
			using var _ = Profiler.Instance.Begin();
#endif
			if (pCBuffer is null)
				return;

			var map = command.Map<Vector3>(pCBuffer, MapType.Write, MapFlags.Discard);
			map[0] = new Vector3(Directions, Quality, Size);
			command.Unmap(pCBuffer, MapType.Write);
			base.OnExecute(command);
		}

		protected override void OnSetImmutableSamplers(IList<ImmutableSamplerDesc> samplers)
		{
			samplers.Add((new ImmutableSamplerDesc()
			{
				Name = TextureNames.MainTexture,
				Sampler = new SamplerStateDesc(TextureFilterMode.Nearest)
			}));
		}

		protected override void OnSetBindings(IShaderResourceBinding binding, IBufferManager bufferManager)
		{
			base.OnSetBindings(binding, bufferManager);

			pCBuffer ??= bufferManager.GetBuffer(BufferGroupType.Material);
			binding.Set(ShaderTypeFlags.Pixel, ConstantBufferNames.Material, pCBuffer);
		}
	}
}
