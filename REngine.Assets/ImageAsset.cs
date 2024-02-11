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
	public class ImageAsset : Asset
	{
		public ImageType ImageType { get; set; } = ImageType.Unknow;
		public string Checksum { get; private set; } = string.Empty;
		public Image Image { get; private set; } = new Image();
		public ImageAsset() { }
		public ImageAsset(string name, ImageType imageType = ImageType.Unknow)
		{
			ImageType = imageType;
		}
		protected override void OnLoad(AssetStream stream)
		{
#if WEB
			throw new NotImplementedException();
#else
			var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			var dataInfo = new ImageDataInfo
			{
				Components = 4,
				Data = img.Data,
				Size = new ImageSize((ushort)img.Width, (ushort)img.Height)
			};
			Image.SetData(dataInfo);
			mSize = img.Data.Length;
#endif
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
#if WEB
				throw new NotImplementedException();
#else
				var writer = new StbImageWriteSharp.ImageWriter();
				var colorComponents = GetColorComponents(Image.Components);
				switch (imgType)
				{
					case ImageType.Bmp:
						writer.WriteBmp(Image.Data, Image.Size.Width, Image.Size.Height, colorComponents, stream);
						break;
					case ImageType.Jpeg:
						writer.WriteJpg(Image.Data, Image.Size.Width, Image.Size.Height, colorComponents, stream, 100);
						break;
					case ImageType.Png:
						writer.WritePng(Image.Data, Image.Size.Width, Image.Size.Height, colorComponents, stream);
						break;
					case ImageType.Tga:
						writer.WriteTga(Image.Data, Image.Size.Width, Image.Size.Height, colorComponents, stream);
						break;
					case ImageType.Hdr:
						writer.WriteHdr(Image.Data, Image.Size.Width, Image.Size.Height, colorComponents, stream);
						break;
				}
#endif
			});
		}

		protected override void OnDispose()
		{
			Image = Image.Empty();
		}

#if !WEB
		private StbImageWriteSharp.ColorComponents GetColorComponents(byte components)
		{
			StbImageWriteSharp.ColorComponents result;
			switch (components)
			{
				case 1:
					result = StbImageWriteSharp.ColorComponents.Grey;
					break;
				case 2:
					result = StbImageWriteSharp.ColorComponents.GreyAlpha;
					break;
				case 3:
					result = StbImageWriteSharp.ColorComponents.RedGreenBlue;
					break;
				case 4:
					result = StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
					break;
				default:
					throw new Exception($"Invalid components count. Components: {components}");
			}

			return result;
		}
#endif
	}
}