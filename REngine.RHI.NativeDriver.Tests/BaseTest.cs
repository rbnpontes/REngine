using REngine.RHI.DiligentDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace REngine.RHI.NativeDriver.Tests
{
	public class BaseTest
	{
		protected Form? MainWindow;
		protected List<IDisposable> Disposables = new List<IDisposable>();

		public BaseTest()
		{
			DriverFactory.OnDriverMessage += HandleLogMessages;
		}

		protected void CreateWindow()
		{
			MainWindow = new()
			{
				Text = "Text Window",
				Name = "TestWindow",
				FormBorderStyle = FormBorderStyle.Sizable,
				ClientSize = new Size(500, 500),
				StartPosition = FormStartPosition.CenterParent,
			};
			MainWindow.Show();
			Disposables.Add(MainWindow);
		}

		protected void CleanDisposables()
		{
			for(int i =0; i < Disposables.Count; i++)
				Disposables[(Disposables.Count - 1) - i].Dispose();
			Disposables.Clear();
		}

		protected (IGraphicsDriver, ISwapChain) CreateDriver(GraphicsBackend backend, SwapChainDesc? swapChainDesc = null)
		{
			var size = MainWindow?.Size ?? new Size(500, 500);
			IGraphicsDriver driver = DriverFactory.Build(new DriverSettings
			{
				Backend = backend,
			}, 
			new NativeWindow { Hwnd = MainWindow?.Handle ?? IntPtr.Zero},
			swapChainDesc ?? new SwapChainDesc { Size = new SwapChainSize(size) },
			out ISwapChain? swapChain);

			Disposables.Add(driver);
			Disposables.Add(swapChain);

			return (driver, swapChain);
		}

		protected void HandleLogMessages(object? sender, MessageEventArgs args)
		{
			switch (args.Severity)
			{
				case DbgMsgSeverity.FatalError:
					{
						Console.WriteLine($"Diligent Engine: {args.Severity} in {args.Function}() ({args.File}, {args.Line}): {args.Message}");
						throw new Exception(args.Message);
					}
				case DbgMsgSeverity.Error:
				case DbgMsgSeverity.Warning:
				case DbgMsgSeverity.Info:
					Console.WriteLine($"Diligent Engine: {args.Severity} {args.Message}");
					break;
			}
		}
	}
}
