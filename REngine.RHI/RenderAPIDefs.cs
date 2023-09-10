using REngine.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct InputLayoutElementDesc
	{
		public uint BufferIndex;
		public uint BufferStride;
		public uint ElementOffset;
		public uint InstanceStepRate;
		public ElementType ElementType;

		public InputLayoutElementDesc()
		{
			this = default(InputLayoutElementDesc);
			BufferStride = uint.MaxValue;
			ElementOffset = uint.MaxValue;
		}
	}

	public struct TextureAddressModes
	{
		public TextureAddressMode U;
		public TextureAddressMode V;
		public TextureAddressMode W;

		public TextureAddressModes()
		{
			U = V = W = TextureAddressMode.Wrap;
		}
		public TextureAddressModes(TextureAddressMode mode)
		{
			U = V = W = mode;
		}
	}

	public struct SamplerStateDesc
	{
		public TextureFilterMode FilterMode;
		public byte Anisotropy;
		public bool ShadowCompare;
		public TextureAddressModes AddressModes;

		public SamplerStateDesc()
		{
			FilterMode = TextureFilterMode.Trilinear;
			Anisotropy = 4;
			ShadowCompare = false;
			AddressModes = new TextureAddressModes();
		}
		public SamplerStateDesc(TextureFilterMode filterMode, TextureAddressMode addressMode)
		{
			FilterMode = filterMode;
			Anisotropy = 0;
			ShadowCompare = false;
			AddressModes = new TextureAddressModes(addressMode);
		}
	}

	public struct ImmutableSamplerDesc
	{
		public string Name;
		public SamplerStateDesc Sampler;

		public ImmutableSamplerDesc()
		{
			Name = string.Empty;
			Sampler = new SamplerStateDesc();
		}
		public ImmutableSamplerDesc(string name, SamplerStateDesc sampler)
		{
			Name = name;
			Sampler = sampler;
		}
	}
}
