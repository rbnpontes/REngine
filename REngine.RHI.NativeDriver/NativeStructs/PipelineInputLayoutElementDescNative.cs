using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct PipelineInputLayoutElementDescNative
	{
		public uint inputIndex;
		public uint bufferIndex;
		public uint bufferStride;
		public uint elementOffset;
		public uint instanceStepRate;
		public byte elementType;
		public byte normalized;

		public static void Fill(in PipelineInputLayoutElementDesc desc, out PipelineInputLayoutElementDescNative output)
		{
			output = new PipelineInputLayoutElementDescNative
			{
				inputIndex = desc.InputIndex,
				bufferIndex = desc.Input.BufferIndex,
				bufferStride = desc.Input.BufferStride,
				elementOffset = desc.Input.ElementOffset,
				instanceStepRate = desc.Input.InstanceStepRate,
				elementType = (byte)desc.Input.ElementType,
				normalized = (byte)(desc.Input.IsNormalized ? 1 : 0),
			};
		}
	}
}
