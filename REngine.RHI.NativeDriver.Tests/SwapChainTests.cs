using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.Tests
{
	[TestFixture]
	public class SwapChainTests : BaseTest
	{
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
		public void MustResize(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			if (MainWindow is null)
				throw new NullReferenceException("MainWindow is null");
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateDriver(backend);

			swapChain.Present(true);
			MainWindow.Invalidate(new Rectangle(0, 0, 0, 1));

			Application.DoEvents();

			MainWindow.Size = new Size(100, 100);

			var wndSize = MainWindow.ClientSize;
			swapChain.Resize((uint)wndSize.Width, (uint)wndSize.Height);

			swapChain.Present(true);
			MainWindow.Invalidate(new Rectangle(0, 0, 0, 1));

			Application.DoEvents();

			Disposables.Add(driver);
			Disposables.Add(swapChain);

			Assert.Pass();
		}
	}
}
