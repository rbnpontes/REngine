using REngine.Assets;

namespace REngine.Asset.Tests
{
	public class ImageAssetTests
	{
		private string pDummyImagePath = Path.Join(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets",
			"you_must_play.jpg"
		);
		private string pOutputImagePath = Path.Join(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets",
			"you_must_play_inverted.jpg"
		);

		[SetUp]
		public void Setup()
		{
		}
		

		[Test]
		public async Task MustLoad()
		{
			using(FileStream stream = new FileStream(pDummyImagePath, FileMode.Open))
			{
				using(ImageAsset asset = new ImageAsset())
				{
					await asset.Load(stream);
					Assert.Greater(asset.Image.Size.Width, 0);
					Assert.Greater(asset.Image.Size.Height, 0);
					Assert.Greater(asset.Image.Components, 0);
					Assert.Greater(asset.Image.Data.Length, 0);
				}
			}

			Assert.Pass();
		}
		[Test]
		public async Task MustSave()
		{
			using(FileStream loadImg = new FileStream(pDummyImagePath, FileMode.Open))
			{
				using(FileStream outputImg = new FileStream(pOutputImagePath, FileMode.OpenOrCreate))
				{
					using(ImageAsset asset = new ImageAsset("you_must_play.jpg", ImageType.Jpeg))
					{
						await asset.Load(loadImg);
						var img = await asset.Image.Flip(Core.Resources.ImageFlip.X);
						asset.Image = img;
						await asset.Save(outputImg);
					}
				}
			}
		}
	}
}