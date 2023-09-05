using Diligent;
using System.Windows.Forms;

namespace REngine.RHI.DiligentDriver.Tests
{
	[TestFixture]
	public class GraphicsDriverTest
	{
		private Form? pMainWindow;
		private List<IDisposable> pDisposables = new List<IDisposable>();

		public GraphicsDriverTest()
		{
			GraphicsFactory.OnMessage += LogMessages;
		}

		[SetUp]
		public void Setup()
		{
			pMainWindow = new Form
			{
				Text = "Test Window",
				Name = "TestWindow",
				FormBorderStyle = FormBorderStyle.Sizable,
				ClientSize = new Size(500, 500),
				StartPosition = FormStartPosition.CenterScreen,
			};
		}
		[TearDown]
		public void Cleanup()
		{
			pDisposables.ForEach(x => x.Dispose());
			pDisposables.Clear();
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
			pMainWindow?.Show();

			ISwapChain swapChain;
			IGraphicsDriver driver = GraphicsFactory.Create(new GraphicsFactoryCreateInfo
			{
				Settings = new GraphicsSettings
				{
					EnableValidation = true,
					Backend = backend,
				},
				WindowHandle = pMainWindow?.Handle ?? IntPtr.Zero,
			}, new SwapChainDesc
			{
				Size = new SwapChainSize(500, 500),
			}, out swapChain);

			swapChain.Present(true);

			pMainWindow?.Invalidate(new Rectangle(0, 0, 0, 1));

			Thread.Sleep(500);
			pMainWindow?.Close();
			Assert.Pass();

			pDisposables.Add(swapChain);
			pDisposables.Add(driver);
			pDisposables.Add(pMainWindow);
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