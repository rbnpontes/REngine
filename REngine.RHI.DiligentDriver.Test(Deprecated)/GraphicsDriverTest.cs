using Diligent;
using System.Windows.Forms;

namespace REngine.RHI.DiligentDriver.Tests
{
	[TestFixture]
	public class GraphicsDriverTest : BaseTest
	{
		[SetUp]
		public void Setup()
		{
			CreateWindow();
			Factory.OnMessage += HandleLogMessages;
		}
		[TearDown]
		public void Cleanup()
		{
			CleanDisposables();
			Factory.OnMessage -= HandleLogMessages;
		}

		[Test, Sequential]
		public void InitGraphics(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateGraphics(backend);

			swapChain.Present(true);

			MainWindow?.Invalidate(new Rectangle(0, 0, 0, 1));

			Thread.Sleep(500);
			MainWindow?.Close();
			Assert.Pass();
		}

		[Test, Sequential]
		public void GetAdapters(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			var adapters = Factory.GetAvailableAdapters(backend);
			Assert.IsNotNull(adapters);

			Console.WriteLine($"Adapters({backend}):");
			Console.WriteLine("- Id, Name, VendorId");
			foreach (var adapter in adapters)
				Console.WriteLine($"- {adapter.Id}, {adapter.Name}, {adapter.VendorId}");

			Assert.Pass();
		}
	}
}