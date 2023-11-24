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
using REngine.RHI;
using REngine.RPI.Constants;

namespace REngine.RPI.Features.PostProcess
{
	public sealed class BlurPostProcess : PostProcessFeature
	{
		private IBuffer? pCBuffer;
		public float Directions { get; set; } = 16;
		public float Quality { get; set; } = 3;
		public float Size { get; set; } = 8;
		protected override ShaderStream OnGetShaderCode()
		{
			return new FileShaderStream(
				Path.Join(EngineSettings.AssetsShadersPostProcessPath, "blur.hlsl")
			);
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			if (pCBuffer is null)
				return;

			var map = command.Map<Vector3>(pCBuffer, MapType.Write, MapFlags.Discard);
			map[0] = new Vector3(Directions, Quality, Size);
			command.Unmap(pCBuffer, MapType.Write);
			base.OnExecute(command);
		}

		protected override void OnSetBindings(IShaderResourceBinding binding, IBufferManager bufferManager)
		{
			base.OnSetBindings(binding, bufferManager);

			pCBuffer ??= bufferManager.GetBuffer(BufferGroupType.Material);
			binding.Set(ShaderTypeFlags.Pixel, ConstantBufferNames.Material, pCBuffer);
		}
	}
}
