using REngine.Core;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace REngine.Windows
{
	public class WindowManager : IWindowManager, IDisposable
	{
		private readonly WindowsBuilder pBuilder = new();
		private readonly List<IWindow> pWindows = new();
		private readonly IEngine pEngine;
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pPipeline;

		private bool pDisposed = false;
		public IReadOnlyList<IWindow> Windows { get => pWindows.AsReadOnly(); }

		public WindowManager(
			EngineEvents events, 
			IEngine engine,
			IExecutionPipeline pipeline)
		{
			pEngine = engine;
			pEngineEvents = events;
			pPipeline = pipeline;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.SystemAware);

			events.OnStart += HandleStart;
			events.OnStop += HandleStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			CloseAllWindows();
			foreach (var window in Windows)
				window.Dispose();
			pWindows.Clear();

			pEngineEvents.OnStop -= HandleStop;

			GC.SuppressFinalize(this);

			pDisposed = true;
		}

		private void HandleStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleStart;
			pPipeline
				.AddEvent(DefaultEvents.WindowsUpdateId, (_) => Update())
				.AddEvent(DefaultEvents.WindowsInvalidateId, (_) => HandleInvalidateEvent());
		}

		private void HandleInvalidateEvent()
		{
			foreach (var wnd in Windows)
				wnd.Update();
		}

		private static void PumpMessages(IntPtr wnd)
		{
			while (User32Api.PeekMessage(out User32Api.MSG msg, wnd, 0, 0, 0x0001))
			{
				User32Api.TranslateMessage(ref msg);
				User32Api.DispatchMessage(ref msg);
			}
		}

		public IWindowManager CloseAllWindows()
		{
			if (pDisposed)
				return this;

			foreach(var window in Windows)
			{
				window.Close();
			}
			return this;
		}

		public IWindowManager Update()
		{
			if (pDisposed)
				return this;

			PumpMessages(IntPtr.Zero);

			int closedWindows = 0;
			foreach(var window in pWindows)
			{
				if (window.IsClosed)
					closedWindows++;
			}


			if (closedWindows == pWindows.Count)
			{
				pEngine.Stop();
			}

			PumpMessages(IntPtr.Zero);
			return this;
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			IWindow window;
			if(createInfo.Control != null)
			{
				Control? ctrl = createInfo.Control as Control;
				if (ctrl is null)
					throw new ArgumentException("Invalid control type at WindowCreationInfo. Control must inherit WinForms Control");
				window = pBuilder.Build(ctrl);
			}
			else
			{
				window = pBuilder.Build();
				
				window.Title = createInfo.Title;
				window.Size = createInfo.Size;

				if (createInfo.Position != null)
					window.Position = createInfo.Position.Value;
			}

			pWindows.Add(window);
			return window;
		}
	}
}
