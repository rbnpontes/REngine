namespace REngine.RHI.NativeDriver.Tests
{
	[TestFixture]
	public class DriverTests : BaseTest
	{
		public DriverTests() : base()
		{
		}

		[SetUp]
		public void Setup()
		{
			CreateWindow();
		}

		[TearDown]
		public void Cleanup()
		{
			CleanDisposables();
		}

		[Test, Sequential]
		public void MustGetAdapters(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			var adapters = DriverFactory.GetAdapters(GraphicsBackend.D3D11);
			Assert.Greater(adapters.Length, 0);

			Console.WriteLine($"Adapters({backend}):");
			Console.WriteLine("- Id, Name, VendorId");
			foreach (var adapter in adapters)
			{
				Assert.IsNotEmpty(adapter.Name);
				Console.WriteLine($"- {adapter.Id}, {adapter.Name}, {adapter.VendorId}");
			}

			Assert.Pass();
		}

		[Test, Sequential]
		public void MustBuild(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateDriver(backend);

			swapChain.Present(true);

			MainWindow?.Invalidate(new Rectangle(0, 0, 0, 1));
			Application.DoEvents();
			MainWindow?.Close();
			Assert.Pass();
		}
	}
}