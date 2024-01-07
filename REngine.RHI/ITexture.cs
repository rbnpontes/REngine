namespace REngine.RHI
{
	public struct TextureSize
	{
		public uint Width;
		public uint Height;

		public TextureSize() 
		{
			Width = Height = 0;
		}
		public TextureSize(uint width, uint height)
		{
			Width = width;
			Height = height;
		}
	}

	public struct TextureClearValue
	{
		public TextureFormat Format;
		public float R;
		public float G;
		public float B;
		public float A;

		public float Depth;
		public byte Stencil;

		public TextureClearValue()
		{
			Format = TextureFormat.Unknown;
			R = G = B = A = 0;
			Depth = 1;
			Stencil = 0;
		}
	}

	public struct TextureDesc
	{
		public string Name;
		public TextureDimension Dimension;
		public TextureSize Size;
		public uint ArraySizeOrDepth;
		public uint Slices { get => ArraySizeOrDepth; }
		public TextureFormat Format;
		public uint MipLevels;
		public uint SampleCount;
		public BindFlags BindFlags;
		public Usage Usage;
		public CpuAccessFlags AccessFlags;
		public TextureFlags Flags;
		public TextureClearValue ClearValue;

		public TextureDesc()
		{
			Name = string.Empty;
			Dimension = TextureDimension.Tex2D;
			Size = new TextureSize();
			ArraySizeOrDepth = 1;
			Format = TextureFormat.Unknown;
			MipLevels = 1;
			SampleCount = 1;
			BindFlags = BindFlags.None;
			Usage = Usage.Default;
			AccessFlags = CpuAccessFlags.None;
			Flags = TextureFlags.None;
			ClearValue = new TextureClearValue();
		}
	}

	public struct TextureViewDesc
	{
		public TextureViewType ViewType;
		public TextureDimension Dimension;
		public TextureFormat Format;
		public uint MostDetailedMip;
		public uint MipLevels;
		public uint FirstSlice;
		public uint SlicesCount;
		public UavAccessFlags AccessFlags;
		public bool AllowMipMapGeneration;
	}

	public interface ITexture : IGPUObject, IGPUState, IGPUHandler
	{
		public TextureDesc Desc { get; }
		public ITextureView GetDefaultView(TextureViewType view);
	}

	public interface ITextureView : IGPUObject
	{
		public ITexture Parent { get; }
		public TextureViewDesc Desc { get; }
		public TextureViewType ViewType { get; }

		public TextureSize Size { get; }
	}
}
