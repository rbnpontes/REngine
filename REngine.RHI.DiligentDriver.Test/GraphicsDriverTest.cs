using Diligent;
using System.Windows.Forms;

namespace REngine.RHI.DiligentDriver.Tests
{
	[TestFixture]
	public class GraphicsDriverTest : BaseTest
	{
		public GraphicsDriverTest()
		{
			GraphicsFactory.OnMessage += LogMessages;
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
			var adapters = GraphicsFactory.GetAvailableAdapters(backend);
			Assert.IsNotNull(adapters);

			Console.WriteLine($"Adapters({backend}):");
			Console.WriteLine("- Id, Name, VendorId");
			foreach (var adapter in adapters)
				Console.WriteLine($"- {adapter.Id}, {adapter.Name}, {adapter.VendorId}");

			Assert.Pass();
		}

		private void LogMessages(object sender, MessageEventArgs args)
		{
			switch (args.Severity) 
			{
				case DbgMsgSeverity.Warning:
				case DbgMsgSeverity.Error:
				case DbgMsgSeverity.FatalError:
					Console.WriteLine($"Diligent Engine: {args.Severity} in {args.Function}() ({args.File}, {args.Line}): {args.Message}");
					break;
				case DbgMsgSeverity.Info:
					Console.WriteLine($"Diligent Engine: {args.Severity} {args.Message}");
					break;
			}
		}
	}
}