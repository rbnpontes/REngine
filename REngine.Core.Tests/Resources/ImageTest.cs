using NUnit.Framework;
using REngine.Core.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Image = REngine.Core.Resources.Image;

namespace REngine.Core.Tests.Resources
{
#if WINDOWS
	// This test only runs on Windows, because Bitmap is only available on this platform.
	[TestFixture]
	public class ImageTest
	{
		[SetUp]
		public void Setup()
		{
			string assetsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets");
			string[] pathsToCreate = new string[] 
			{
				Path.Combine(assetsPath, "flip_result"),
				Path.Combine(assetsPath, "resize_factor_result"),
				Path.Combine(assetsPath, "resize_result")
			};

			foreach(var path in pathsToCreate)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}
		}

		private Image ReadImage(string path)
		{
			var img = new Image();
			using (var bitmap = new Bitmap(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets", path)))
			{
				var bitmapData = bitmap.LockBits(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.ReadOnly,
					PixelFormat.Format32bppArgb
				);
				ImageDataInfo dataInfo = new ImageDataInfo
				{
					Size = new ImageSize((ushort)bitmapData.Width, (ushort)bitmapData.Height),
					Data = new byte[bitmapData.Stride * bitmapData.Height]
				};


				Marshal.Copy(bitmapData.Scan0, dataInfo.Data, 0, bitmapData.Stride * bitmapData.Height);
				bitmap.UnlockBits(bitmapData);

				img.SetData(dataInfo);
			}
			return img;
		}
		private void WriteImage(string path, Image img)
		{
			using (var bitmap = new Bitmap(img.Size.Width, img.Size.Height))
			{
				var bitmapData = bitmap.LockBits(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.WriteOnly,
					PixelFormat.Format32bppArgb
				);
				Marshal.Copy(img.Data, 0, bitmapData.Scan0, img.Data.Length);
				bitmap.UnlockBits(bitmapData);

				var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", path);
				if (File.Exists(output))
					File.Delete(output);
				bitmap.Save(output, ImageFormat.Jpeg);
			}
		}

		[Test, Sequential]
		public async Task MustFlipImage(
			[Values(
				ImageFlip.X,
				ImageFlip.Y,
				ImageFlip.XY
			)]
				ImageFlip flip
			)
		{
			var img = ReadImage("christimas_tree.jpg");
			img = await img.Flip(flip);

			WriteImage($"flip_result/inverted_{flip}_christimas_tree.jpg", img);
			Assert.Pass();
		}

		[Test, Sequential]
		public async Task MustResizeImageByFactor(
			[Values(
				2, // 2x size
				1.5, // 1.5x size
				0.5, // 0.5x size
				0.25,
				0.1
			)]
				double factor
			)
		{
			var img = ReadImage("christimas_tree.jpg");
			img = await img.Resize(factor);

			WriteImage($"resize_factor_result/{factor}x_christimas_tree.jpg", img);
			Assert.Pass();
		}
		[Test,Sequential]
		public async Task MustResizeImage([Range(0, 2)] int idx)
		{
			ImageSize[] sizes = new ImageSize[]
			{
				new ImageSize(500, 500),
				new ImageSize(200, 800),
				new ImageSize(800, 200)
			};

			var img = ReadImage("christimas_tree.jpg");
			img = await img.Resize(sizes[idx]);

			WriteImage($"resize_result/{sizes[idx]}_christimas_tree.jpg", img);
			Assert.Pass();
		}
	}
#endif
}
