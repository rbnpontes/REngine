using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class TextureAdapter
	{
		public void Fill(in TextureDesc desc, out Diligent.TextureDesc output)
		{
			output = new Diligent.TextureDesc
			{
				Name = desc.Name,
				Type = (ResourceDimension)desc.Dimension,
				Width = desc.Size.Width,	
				Height = desc.Size.Height,
				ArraySizeOrDepth = desc.ArraySizeOrDepth,
				Format = (Diligent.TextureFormat)desc.Format,
				MipLevels = desc.MipLevels,
				SampleCount = desc.SampleCount,
				BindFlags = (Diligent.BindFlags)desc.BindFlags,
				Usage= (Diligent.Usage)desc.Usage,
				CPUAccessFlags = (Diligent.CpuAccessFlags)desc.AccessFlags,
				MiscFlags = (Diligent.MiscTextureFlags)desc.Flags,
				ClearValue = new Diligent.OptimizedClearValue
				{
					Color = new Vector4(desc.ClearValue.R, desc.ClearValue.G, desc.ClearValue.B, desc.ClearValue.A),
					Format = (Diligent.TextureFormat)(desc.ClearValue.Format == TextureFormat.Unknown ? desc.Format : desc.ClearValue.Format)
				}
			};
		}

		public void Fill(in Diligent.TextureDesc desc, out TextureDesc output)
		{
			output = new TextureDesc
			{
				Name = desc.Name,
				Dimension = (TextureDimension)desc.Type,
				Size = new TextureSize(desc.Width, desc.Height),
				ArraySizeOrDepth = desc.ArraySizeOrDepth,
				Format = (TextureFormat)desc.Format,
				MipLevels = desc.MipLevels,
				SampleCount = desc.SampleCount,
				BindFlags = (BindFlags)desc.BindFlags,
				Usage = (Usage)desc.Usage,
				AccessFlags = (CpuAccessFlags)desc.CPUAccessFlags,
				Flags = (TextureFlags)desc.MiscFlags,
				ClearValue = new TextureClearValue
				{
					R = desc.ClearValue.Color.X,
					G = desc.ClearValue.Color.Y,
					B = desc.ClearValue.Color.Z,
					A = desc.ClearValue.Color.W,
					Format = (TextureFormat)desc.ClearValue.Format
				}
			};
		}
		public void Fill(IEnumerable<ITextureData> data, out Diligent.TextureData output)
		{
			TextureSubResData[] subresources = new TextureSubResData[data.Count()];
			Parallel.For(0, subresources.Length, i =>
			{
				var texData = data.ElementAt((int)i);
				BufferImpl? buffer = (texData.SrcBuffer as BufferImpl);

				subresources[i] = new TextureSubResData
				{
					Data = texData.Data,
					DepthStride = texData.DepthStride,
					Stride = texData.Stride,
					SrcBuffer = buffer?.Handle as Diligent.IBuffer,
					SrcOffset = texData.SrcOffset
				};
			});

			output = new Diligent.TextureData { SubResources = subresources };
		}
	
		public void Fill(in TextureViewDesc desc, out Diligent.TextureViewDesc output)
		{
			output = new Diligent.TextureViewDesc
			{
				ViewType = (Diligent.TextureViewType)desc.ViewType,
				TextureDim = (ResourceDimension)desc.Dimension,
				Format = (Diligent.TextureFormat)desc.Format,
				MostDetailedMip = desc.MostDetailedMip,
				NumMipLevels = desc.MipLevels,
				FirstSlice = desc.FirstSlice,
				NumSlices = desc.SlicesCount,
				AccessFlags = (UavAccessFlag)desc.AccessFlags,
				Flags = desc.AllowMipMapGeneration ? TextureViewFlags.AllowMipMapGeneration : TextureViewFlags.None
			};
		}
		public void Fill(in Diligent.TextureViewDesc desc, out TextureViewDesc output)
		{
			output = new TextureViewDesc
			{
				ViewType = (TextureViewType)desc.ViewType,
				Dimension = (TextureDimension)desc.TextureDim,
				Format = (TextureFormat)desc.Format,
				MostDetailedMip = desc.MostDetailedMip,
				MipLevels = desc.NumMipLevels,
				FirstSlice = desc.FirstSlice,
				SlicesCount = desc.NumSlices,
				AccessFlags = (UavAccessFlags)desc.AccessFlags,
				AllowMipMapGeneration = (desc.Flags & TextureViewFlags.AllowMipMapGeneration) != 0
			};
		}
	}
}
