using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable All

namespace REngine.RHI.NativeDriver.NativeStructs
{
	public struct ComputePipelineDescDTO
	{
		public IntPtr name;
		public IntPtr samplers;
		public byte numSamplers;
		public IntPtr shader;
		public IntPtr pscache;

		public static void Fill(in ComputePipelineDesc desc, out ComputePipelineDescDTO output)
		{
			output = new ComputePipelineDescDTO
			{
				name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(desc.Name),
				numSamplers = (byte)desc.Samplers.Count,
				shader = desc.ComputeShader?.Handle ?? IntPtr.Zero,
				pscache = desc.PSCache?.Handle ?? IntPtr.Zero,
			};
		}
	}
}
