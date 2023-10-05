using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.NativeStructs
{
	internal struct ImmutableSamplerDescNative
	{
		public IntPtr name;
		public byte sampler_filterMode;
		public byte sampler_anisotropy;
		public byte sampler_shadowCmp;
		public byte sampler_addressMode_u;
		public byte sampler_addressMode_v;
		public byte sampler_addressMode_w;

		public static void Fill(in ImmutableSamplerDesc desc, out ImmutableSamplerDescNative output)
		{
			output = new ImmutableSamplerDescNative
			{
				name = string.IsNullOrEmpty(desc.Name) ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(desc.Name),
				sampler_filterMode = (byte)desc.Sampler.FilterMode,
				sampler_anisotropy = desc.Sampler.Anisotropy,
				sampler_shadowCmp = (byte)(desc.Sampler.ShadowCompare ? 1 : 0),
				sampler_addressMode_u = (byte)desc.Sampler.AddressModes.U,
				sampler_addressMode_v = (byte)desc.Sampler.AddressModes.V,
				sampler_addressMode_w = (byte)desc.Sampler.AddressModes.W,
			};
		}
	}
}
