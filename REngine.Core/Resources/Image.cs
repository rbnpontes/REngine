using REngine.Core.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Resources
{
	public class ImageAtlas 
	{
		public Image Image { get; set; }
		public Rectangle[] Items { get; set; } = Array.Empty<Rectangle>();
		public ImageAtlas(Image image)
		{
			Image = image;
		}
	}

	/// <summary>
	/// This struct defines Image size
	/// In Width and Height engine supports values to 65535 as max size
	/// And 255 in depth size.
	/// In the worst case, Image will support to at least 4GB of pixel data.
	/// </summary>
	public struct ImageSize
	{
		public ushort Width;
		public ushort Height;
		public byte Depth;

		public ImageSize()
		{
			Width = 1;
			Height = 1;
			Depth = 1;
		}

		public ImageSize(ushort width, ushort height, byte depth = 1)
		{
			Width = width;
			Height = height;
			Depth = depth;
		}

		public ImageSize(Size size, byte depth = 1)
		{
			Width = (ushort)size.Width;
			Height = (ushort)size.Height;
			Depth = depth;
		}
		
		public ImageSize(Vector2 size, byte depth = 1)
		{
			Width = (ushort)size.X;
			Height = (ushort)size.Y;
			Depth = depth;
		}

		public ImageSize(Vector3 size)
		{
			Width = (ushort)size.X;
			Height = (ushort)size.Y;
			Depth = (byte)size.Y;
		}

		public override string ToString()
		{
			return $"ImageSize({Width}, {Height}, {Depth})";
		}
	}

	public struct ImageDataInfo
	{
		public ImageSize Size;
		public byte[] Data;
		public byte Components;

		public ImageDataInfo()
		{
			Size = new ImageSize();
			Data = new byte[1];
			Components = 4;
		}
	}

	[Flags]
	public enum ImageFlip
	{
		None = 0,
		X = 1 << 0,
		Y = 1 << 1,
		Z = 1 << 2,
		XY = X | Y,
		All = X | Y | Z
	}

	public class Image
	{
		public ImageSize Size { get; private set; } = new ImageSize();
		public byte[] Data { get; private set; } = new byte[0];
		public byte Components { get; private set; } = 0;

		public uint Stride { get => (uint)(Size.Width * Size.Depth * Components); }

		private int GetPixelIdx(ushort x, ushort y, byte z)
		{
			x = Math.Min(x, (ushort)(Size.Width - 1));
			y = Math.Min(y, (ushort)(Size.Height - 1));
			z = Math.Min(z, (byte)(Size.Depth - 1));

			return (z * Size.Width * Size.Height + y * Size.Width + x) * Components;
		}

		public Image SetPixel(int color, ushort x, ushort y, byte z = 0)
		{
			byte[] colorChannels = new byte[]
			{
				(byte)(color >> 24),
				(byte)(color >> 16), 
				(byte)(color >> 8), 
				(byte)(color & 0xFF), 
			};

			int pixelIdx = GetPixelIdx(x, y, z);
			for (int i = 0; i < Components; ++i)
				Data[pixelIdx + i] = colorChannels[i];
			return this;
		}
		public Image SetPixel(int color, double x, double y, double z = 0.0)
		{
			SetPixel(color, (ushort)(x * Size.Width), (ushort)(y * Size.Height), (byte)(z * Size.Depth));
			return this;
		}
		public Image SetPixel(Color color, ushort x, ushort y, byte z = 0)
		{
			return SetPixel(color.ToArgb(), x, y, z);
		}
		public Image SetPixel(Color color, double x, double y, double z = 1.0)
		{
			return SetPixel(color.ToArgb(), x, y, z);
		}
		public Color GetPixel(ushort x, ushort y, byte z = 0)
		{
			byte[] data = Data;
			byte a = 0;
			byte r = 0;
			byte g = 0;
			byte b = 0;

			int pixelIdx = GetPixelIdx(x, y, z);

			if (Components == 4)
				a =	 data[pixelIdx];
			if (Components >= 3)
				r = data[pixelIdx + 1];
			if(Components >= 2)
			{
				g = data[pixelIdx + 2];
				b = data[pixelIdx + 3];
			}
			
			if(Components == 1)
				a = r = g = b = data[pixelIdx];

			return Color.FromArgb(a, r, g, b);
		}
		/// <summary>
		/// Get Bilinear Pixel.
		/// </summary>
		/// <param name="x">value must be in the range 0 ~ 1</param>
		/// <param name="y">value must be in the range 0 ~ 1</param>
		/// <returns></returns>
		public Color GetBilinearPixel(double x, double y)
		{
			// Bilinear Algorithm is quite simple
			// Basicaly we must to sample each corner at position (x, y).
			// 
			// Your pixel is in a box like this
			// top_left ---------- top_right
			// |						   |
			// |						   |
			// |--------- (x, y) ----------|
			// |						   |
			// |						   |
			// bottom_left ---- bottom_right
			//
			// The distance between top_left and top_right will be a value range 0 ~ 1
			// Then we use this value to Lerp between top_left and top_right and save as top_color
			// and same for bottom_left and bottom_right and save as bottom_color
			// And finaly, we lerp again values but using top_color and bottom_clor and y corner distance

			// Sample Coords to image coords
			x = Math.Clamp(x * Size.Width - 0.5, 0.0, Size.Width - 1.0);
			y = Math.Clamp(y * Size.Height - 0.5, 0.0, Size.Height - 1.0);

			// round coords to int
			ushort xI = (ushort)x;
			ushort yI = (ushort)y;
			// extract fractional part of coords
			double xF = Mathf.Fract(x);
			double yF = Mathf.Fract(y);

			// sample corners and get bilinear pixel
			Color topColor = GetPixel(xI, yI).Lerp(GetPixel((ushort)(xI + 1), yI), xF);
			Color bottomColor = GetPixel(xI, (ushort)(yI + 1)).Lerp(GetPixel((ushort)(xI + 1), (ushort)(yI + 1)), xF);
			return topColor.Lerp(bottomColor, yF);
		}
		public Color GetTrilinearPixel(double x, double y, double z)
		{
			// There's no reason to do Trilinear algorithm
			// when depth is 1
			if (Size.Depth == 1)
				return GetBilinearPixel(x, y);

			// Trilenar algorithm works in the same way of bilinear
			// But instead of blend between 2 coords (X, Y) we blend with Z coord too.

			x = Math.Clamp(x * Size.Width - 0.5, 0.0, Size.Width - 1.0);
			y = Math.Clamp(y * Size.Height - 0.5, 0.0, Size.Height - 1.0);
			z = Math.Clamp(z * Size.Depth - 0.5, 0.0, Size.Depth - 1.0);

			ushort xI = (ushort)x;
			ushort yI = (ushort)y;
			byte zI = (byte)z;

			if (zI == Size.Depth - 1)
				return GetBilinearPixel(x, y);

			double xF = Mathf.Fract(x);
			double yF = Mathf.Fract(y);
			double zF = Mathf.Fract(z);

			Color topColorNear = GetPixel(xI, yI, zI).Lerp(GetPixel((ushort)(xI + 1), yI, zI), xF);
			Color bottomColorNear = GetPixel(xI, (ushort)(yI + 1), zI).Lerp(GetPixel((ushort)(xI + 1), (ushort)(yI + 1), zI), xF);
			Color colorNear = topColorNear.Lerp(bottomColorNear, yF);
			Color topColorFar = GetPixel(xI, yI, (byte)(zI + 1)).Lerp(GetPixel((ushort)(xI + 1), yI, (byte)(zI + 1)), xF);
			Color bottomColorFar = GetPixel(xI, (ushort)(yI + 1), (byte)(zI + 1)).Lerp(GetPixel((ushort)(xI + 1), (ushort)(yI + 1), (byte)(zI + 1)), xF);
			Color colorFar = topColorFar.Lerp(bottomColorFar, yF);
			return colorNear.Lerp(colorFar, zF);
		}
		/// <summary>
		/// Get Encoded 32 bits int pixel color
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public int GetArgbPixel(ushort x, ushort y, byte z = 0)
		{
			byte[] data = Data;
			byte a = 0;
			byte r = 0;
			byte g = 0;
			byte b = 0;

			int pixelIdx = GetPixelIdx(x, y, z);

			if (Components == 4)
				a = data[pixelIdx];
			if (Components >= 3)
				r = data[pixelIdx + 1];
			if (Components >= 2)
			{
				g = data[pixelIdx + 2];
				b = data[pixelIdx + 3];
			}

			if (Components == 1)
				a = r = g = b = data[pixelIdx];

			return a << 24 | r << 16 | g << 8 | b;
		}

		public Image SetData(ImageDataInfo dataInfo)
		{
#if DEBUG
			// Skip this check on Release mode
			// Usually all data will be checked before call this method
			// This check is more usefull on debug mode.

			if (dataInfo.Size.Width == 0)
				throw new ArgumentException("Image Width cannot be 0");
			if (dataInfo.Size.Height == 0)
				throw new ArgumentException("Image Height cannot be 0");
			if (dataInfo.Size.Depth == 0)
				throw new ArgumentException("Image Depth cannot be 0");
			if (dataInfo.Components == 0 || dataInfo.Components > 4)
				throw new ArgumentException($"Image Components must be in the range of [1 ~ 4]. Current {dataInfo.Components}");

			uint size = (uint)(dataInfo.Size.Width * dataInfo.Size.Height * dataInfo.Size.Depth * dataInfo.Components);

			if (dataInfo.Data.Length != size)
				throw new ArgumentException("Invalid Data Size.");
#endif
			Size = dataInfo.Size;
			Data = dataInfo.Data;
			Components = dataInfo.Components;
			return this;
		}
		public Image SetData(byte[] data)
		{
			if (data.Length != Data.Length)
				throw new ArgumentException("Invalid Data Size.");
			Data = data;
			return this;
		}

		public Image Clear(byte r, byte g, byte b, byte a)
		{
			byte[] colorChannels = new byte[4] { a, r, g, b };

			Data = Data
				.AsParallel()
				.Select((_, idx) => colorChannels[idx % Components])
				.ToArray();

			return this;
		}
		public Image Clear(int color)
		{
			return Clear((byte)(color >> 16), (byte)(color >> 8), (byte)(color & 0xFF), (byte)(color >> 24));
		}
		public Image Clear(Color color)
		{
			return Clear(color.R, color.G, color.B, color.A);
		}
	
		public Task<Image> Resize(ImageSize size)
		{
			if(size.Width == Size.Width && size.Height == Size.Height && size.Depth == Size.Depth)
				return Task.Run(()=> this);

			size.Width = Math.Max(size.Width, (ushort)1);
			size.Height = Math.Max(size.Height, (ushort)1);
			size.Depth = Math.Max(size.Depth, (byte)1);

			return Task.Run(() =>
			{
				Image newImg = new Image();
				newImg.Data = new byte[size.Width * size.Height * size.Depth * Components];
				newImg.Size = size;
				newImg.Components = Components;

				for(int x = 0; x < size.Width; ++x)
				{
					for(int y = 0; y < size.Height; ++y)
					{
						for(int z = 0; z < size.Depth; ++z)
						{
							Color pixel = GetTrilinearPixel(x / (double)size.Width, y / (double)size.Height, z / (double)size.Depth);
							newImg.SetPixel(pixel, (ushort)x, (ushort)y, (byte)z);
						}
					}
				}

				return newImg;
			});
		}
		public Task<Image> Resize(double factor)
		{
			ImageSize size = new ImageSize((ushort)(Size.Width * factor), (ushort)(Size.Height * factor), (byte)(Size.Depth * factor));
			// Only scale depth if is greater than 1
			if (Size.Depth == 1)
				size.Depth = 1;
			return Resize(size);
		}

		public Task<Image> Flip(ImageFlip flags)
		{
			if (flags == ImageFlip.None)
				return Task.Run(() => this);
			return Task.Run(() =>
			{
				Image newImg = new Image();
				newImg.Data = new byte[Data.Length];
				newImg.Size = Size;
				newImg.Components = Components;

				for(int x = 0; x < Size.Width; ++x)
				{
					for(int y =0; y < Size.Height; ++y)
					{
						for(int z = 0; z < Size.Depth; ++z)
						{
							ushort invX = (ushort)x;
							ushort invY = (ushort)y;
							byte invZ = (byte)z;

							if((flags & ImageFlip.X) != 0)
								invX = (ushort)(Size.Width - x);
							if ((flags & ImageFlip.Y) != 0)
								invY = (ushort)(Size.Height - y);
							if ((flags & ImageFlip.Z) != 0)
								invX = (ushort)(Size.Depth - z);

							newImg.SetPixel(
								GetPixel((ushort)x, (ushort)y, (byte)z),
								invX, invY, invZ
							);
						}
					}
				}

				return newImg;
			});
		}

		/// <summary>
		/// Merge all images into a single image atlas
		/// </summary>
		/// <param name="images">Images List</param>
		/// <param name="breakAt">At each x items, a new row will be created.</param>
		/// <returns>Returns merged atlas image</returns>
		public static ImageAtlas MakeAtlas(IEnumerable<Image> images, byte spacing = 0, byte breakAt = 5)
		{
			ushort atlasWidth = 0;

			ushort nextWidth = spacing;
			ushort nextHeight = nextWidth;
			byte components = 0;

			ushort currCol = 0;
			ushort maxRowHeight = 0;

			// Calculate Atlas size
			foreach (var img in images)
			{
				components = Math.Max(img.Components, components);

				nextWidth += (ushort)(img.Size.Width + spacing);
				maxRowHeight = Math.Max(maxRowHeight, img.Size.Height);

				atlasWidth = Math.Max(nextWidth, atlasWidth);

				++currCol;
				if(currCol >= breakAt)
				{
					nextHeight += (ushort)(maxRowHeight + spacing);
					nextWidth = spacing;
					maxRowHeight = 0;
					currCol = 0;
				}
			}

			nextHeight += (ushort)(maxRowHeight + spacing);

			Image result = new Image();
			result.SetData(new ImageDataInfo
			{
				Components = components,
				Size = new ImageSize(atlasWidth, nextHeight),
				Data = new byte[atlasWidth * nextHeight * components]
			});

			currCol = maxRowHeight = 0;
			
			List<Rectangle> bounds = new();

			nextWidth = nextHeight = spacing;

			foreach(var img in images)
			{
				// Copy image item to image atlas
				for(ushort y = 0; y < img.Size.Height; ++y)
				{
					for(ushort x = 0; x < img.Size.Width; ++x)
					{
						Color color = img.GetPixel(x, y);
						result.SetPixel(
							color,
							(ushort)(nextWidth + x),
							(ushort)(nextHeight + y)
						);
					}
				}
				
				// Calculate Max Row Height
				maxRowHeight = Math.Max(maxRowHeight, img.Size.Height);
				nextWidth += (ushort)(img.Size.Width + spacing);

				bounds.Add(new Rectangle(nextWidth, nextHeight, img.Size.Width, img.Size.Height));

				++currCol;
				if(currCol >= breakAt)
				{
					// Make next row
					nextHeight += (ushort)(maxRowHeight + spacing);
					maxRowHeight = 0;
					nextWidth = spacing;
					currCol = 0;
				}
			}

			//for(ushort y = 0; y < result.Size.Height; ++y)
			//{
			//	for(ushort x =0; x < result.Size.Width; ++x)
			//	{
			//		Color color = result.GetPixel(x, y);
			//		int value = color.R + color.G + color.B + color.A;
			//		if (value != 0)
			//			System.Diagnostics.Debugger.Break();
			//	}
			//}

			return new ImageAtlas(result) { Items = bounds.ToArray() };
		}
	}
}
