using REngine.Base.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Tests
{
	public class BaseTest
	{
		protected Form? MainWindow;
		protected List<IDisposable> Disposables = new List<IDisposable>();
		protected MockServiceProvider ServiceProvider;
		protected GraphicsFactory Factory;

		public BaseTest()
		{
			ServiceProvider = new MockServiceProvider();
			ServiceProvider.AddService(new GraphicsSettings());
			Factory = new GraphicsFactory(ServiceProvider);
		}

		protected void CreateWindow()
		{
			MainWindow = new Form
			{
				Text = "Test Window",
				Name = "TestWindow",
				FormBorderStyle = FormBorderStyle.Sizable,
				ClientSize = new Size(500, 500),
				StartPosition = FormStartPosition.CenterScreen,
			};
			MainWindow.Show();
			Disposables.Add(MainWindow);
		}

		protected void CleanDisposables()
		{
			for(int i =0; i < Disposables.Count; ++i)
				Disposables[(Disposables.Count - 1) - i].Dispose();
			Disposables.Clear();
		}

		protected (IGraphicsDriver, ISwapChain) CreateGraphics(GraphicsBackend backend, SwapChainDesc? swapChainDesc = null)
		{
			var size = MainWindow?.Size ?? new Size(500, 500);

			ISwapChain swapChain;
			IGraphicsDriver driver = Factory.Create(new GraphicsFactoryCreateInfo
			{
				Settings = new GraphicsDriverSettings
				{
					EnableValidation = true,
					Backend = backend,
				},
				WindowHandle = MainWindow?.Handle ?? IntPtr.Zero,
			}, swapChainDesc ?? new SwapChainDesc
			{
				Size = new SwapChainSize((uint)size.Width, (uint)size.Height),
			}, out swapChain);

			Disposables.Add(driver);
			Disposables.Add(swapChain);

			return (driver, swapChain);
		}

		protected void HandleLogMessages(object sender, MessageEventArgs args)
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
