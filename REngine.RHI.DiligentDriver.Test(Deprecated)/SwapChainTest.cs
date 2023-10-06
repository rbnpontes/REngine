using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Tests
{
	[TestFixture]
	public class SwapChainTest : BaseTest
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
		public void Resize(
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
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateGraphics(backend);

			swapChain.Present(true);
			MainWindow.Invalidate(new Rectangle(0, 0, 0, 1));

			Thread.Sleep(500);

			MainWindow.Size = new Size(100, 100);

			var wndSize = MainWindow.ClientSize;
			swapChain.Resize((uint)wndSize.Width, (uint)wndSize.Height);
			
			swapChain.Present(true);
			MainWindow.Invalidate(new Rectangle(0, 0, 0, 1));

			Thread.Sleep(500);

			Disposables.Add(driver);
			Disposables.Add(swapChain);

			Assert.Pass();
		}

		[Test, Sequential]
		public void AcquireColorBuffers(
			[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			HashSet<ITextureView> textures = new HashSet<ITextureView>();

			(IGraphicsDriver driver, ISwapChain swapChain) = CreateGraphics(backend);

			for(int i =0; i < swapChain.BufferCount; ++i)
			{
				textures.Add(swapChain.ColorBuffer);
				swapChain.Present(true);
			}

			for(int i = 0; i < swapChain.BufferCount; ++i)
			{
				Assert.IsTrue(textures.Contains(swapChain.ColorBuffer));
			}

			Assert.Pass();
		}
		[Test,Sequential]
		public void AcquireDepthBuffer(
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

			Assert.NotNull(swapChain.DepthBuffer);
			Assert.Pass();
		}
		[Test, Sequential]
		[Description("must return null depth buffer if depth buffer format is Unknow at swap chain creation.")]
		public void NullDepthBuffer(
	[Values(
#if WINDOWS
				GraphicsBackend.D3D11,
				GraphicsBackend.D3D12,
#endif
				GraphicsBackend.Vulkan,
				GraphicsBackend.OpenGL
			)] GraphicsBackend backend)
		{
			var size = MainWindow?.ClientSize ?? new Size(500, 500);
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateGraphics(backend, new SwapChainDesc
			{
				Size = new SwapChainSize((uint)size.Width, (uint)size.Height),
				Formats = new SwapChainFormats(TextureFormat.RGBA8UNorm, TextureFormat.Unknown),
			});

			Assert.IsNull(swapChain.DepthBuffer);
		}
	}
}
