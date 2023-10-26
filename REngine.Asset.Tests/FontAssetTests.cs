using REngine.Assets;
using REngine.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Asset.Tests
{
	public class FontAssetTests
	{
		private string pFontPath = Path.Join(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets/Fonts/Anonymous Pro.ttf"
		);

		private string pOutputFontPath = Path.Join(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets/Fonts/"
		);

		[Test]
		public async Task MustLoad()
		{
			using(FileStream stream = new FileStream(pFontPath, FileMode.Open))
			{
				using(FontAsset asset = new())
				{
					await asset.Load(stream);
				}
			}

			Assert.Pass();
		}

		[Test]
		public async Task MustGetGlyph()
		{
			Image img;

			using(FileStream stream = new FileStream(pFontPath, FileMode.Open))
			{
				using(FontAsset asset = new())
				{
					await asset.Load(stream);
					img = asset.GetGlyph(65/*Load 'A' char*/);
				}
			}

			string fileOutput = "A_font.jpg";
			using(FileStream stream = new FileStream(Path.Join(pOutputFontPath, fileOutput), FileMode.OpenOrCreate))
			{
				using(ImageAsset asset = new ImageAsset(fileOutput, ImageType.Jpeg))
				{
					asset.Image = img;
					await asset.Save(stream);
				}
			}

			Assert.Pass();
		}

		[Test]
		public async Task MustBuildAtlas()
		{
			Image img;
			using (FileStream stream = new FileStream(pFontPath, FileMode.Open))
			{
				using (FontAsset asset = new())
				{
					await asset.Load(stream);
					var font = asset.Font;
					img = font.Atlas;
				}
			}

			string fileOutput = "atlas.jpg";
			using (FileStream stream = new FileStream(Path.Join(pOutputFontPath, fileOutput), FileMode.OpenOrCreate))
			{
				using (ImageAsset asset = new ImageAsset(fileOutput, ImageType.Jpeg))
				{
					asset.Image = img;
					await asset.Save(stream);
				}
			}

			Assert.Pass();
		}
	}
}
