using REngine.Core.Resources;
using StbImageSharp;
using System.Security.Cryptography;

namespace REngine.Assets
{
	public enum ImageType
	{
		Unknow,
		Bmp,
		Jpeg,
		Png,
		Tga,
		Hdr
	}
	public class ImageAsset : IAsset
	{
		public ImageType ImageType { get; set; } = ImageType.Unknow;
		public string Name { get; set; } = string.Empty;
		public string Checksum { get; private set; } = string.Empty;
		public Image Image { get; set; } = new Image();
		public int Size { get => Image.Data.Length; }

		public ImageAsset() { }
		public ImageAsset(string name, ImageType imageType = ImageType.Unknow)
		{
			Name = name;
			ImageType = imageType;
		}

		public Task Load(Stream stream)
		{
			return Task.Run(() =>
			{
				ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
				ImageDataInfo dataInfo = new ImageDataInfo
				{
					Components = 4,
					Data = img.Data,
					Size = new ImageSize((ushort)img.Width, (ushort)img.Height)
				};
				Image.SetData(dataInfo);
			});
		}

		public Task Save(ImageType imageType, Stream stream)
		{
			ImageType = imageType;
			return Save(stream);
		}

		public Task Save(Stream stream)
		{
			var imgType = ImageType;
			if (imgType == ImageType.Unknow)
				throw new Exception("You must set a ImageType before save, Or call specialized Save method from ImageAsset.");
			return Task.Run(() =>
			{
				StbImageWriteSharp.ImageWriter writer = new StbImageWriteSharp.ImageWriter();
				
				switch (imgType)
				{
					case ImageType.Bmp:
						writer.WriteBmp(Image.Data, Image.Size.Width, Image.Size.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
						break;
					case ImageType.Jpeg:
						writer.WriteJpg(Image.Data, Image.Size.Width, Image.Size.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 100);
						break;
					case ImageType.Png:
						writer.WritePng(Image.Data, Image.Size.Width, Image.Size.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
						break;
					case ImageType.Tga:
						writer.WriteTga(Image.Data, Image.Size.Width, Image.Size.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
						break;
					case ImageType.Hdr:
						writer.WriteHdr(Image.Data, Image.Size.Width, Image.Size.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
						break;
				}
			});
		}

		public void Dispose()
		{
			Image = new Image();
		}

	}
}