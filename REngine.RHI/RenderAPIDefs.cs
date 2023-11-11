using REngine.Core.Collections;
using REngine.Core.Mathematics;
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
		/// <summary>
		/// To calculate auto stride, use uint.MaxValue.
		/// </summary>
		public uint BufferStride;
		/// <summary>
		/// To calculate auto element offset, use uint.MaxValue.
		/// </summary>
		public uint ElementOffset;
		public uint InstanceStepRate;
		public ElementType ElementType;
		public bool IsNormalized;

		public InputLayoutElementDesc()
		{
			this = default(InputLayoutElementDesc);
			BufferStride = uint.MaxValue;
			ElementOffset = uint.MaxValue;
			IsNormalized = false;
		}

		public ulong ToHash()
		{
			ulong hash = (BufferIndex << 32) | BufferStride;
			hash = Hash.Combine(hash, ((ulong)ElementOffset << 32) | InstanceStepRate);
			hash = Hash.Combine(hash, (ulong)ElementType);
			return Hash.Combine(hash, IsNormalized ? 1UL : 0UL);
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

		public ulong ToHash()
		{
			return Hash.Combine((byte)U, (byte)V, (byte)W);
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
		public SamplerStateDesc(TextureFilterMode filterMode, TextureAddressMode addressMode = TextureAddressMode.Clamp)
		{
			FilterMode = filterMode;
			Anisotropy = 0;
			ShadowCompare = false;
			AddressModes = new TextureAddressModes(addressMode);
		}

		public ulong ToHash()
		{
			ulong hash = Hash.Combine((byte)FilterMode, Anisotropy, ShadowCompare ? 1U : 0U);
			return Hash.Combine(hash, AddressModes.ToHash());
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

		public ulong ToHash()
		{
			return Hash.Combine(Hash.Digest(Name), Sampler.ToHash());
		}
	}
}
