namespace REngine.RHI.NativeDriver.Tests
{
	public class DriverTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void MustGetAdapters()
		{
			DriverFactory factory = new DriverFactory();
			Assert.Greater(DriverFactory.GetAdapters(GraphicsBackend.D3D11).Length, 0);
			Assert.Pass();
		}
	}
}