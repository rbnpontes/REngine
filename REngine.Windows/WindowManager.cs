using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	public class WindowManager : IWindowManager, IDisposable
	{
		private WindowsBuilder pBuilder = new WindowsBuilder();
		private List<IWindow> pWindows = new List<IWindow>();
		private IEngine pEngine;
		private EngineEvents pEngineEvents;

		public IReadOnlyList<IWindow> Windows { get => pWindows.AsReadOnly(); }

		public WindowManager(EngineEvents events, IEngine engine)
		{
			pEngine = engine;
			pEngineEvents = events;

			events.OnStart += HandleStart;
			events.OnStop += HandleStop;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
		}

		public void Dispose()
		{
			CloseAllWindows();
			foreach (var window in Windows)
				window.Dispose();
			pWindows.Clear();

			pEngineEvents.OnStart -= HandleStart;
			pEngineEvents.OnStop -= HandleStop;

			GC.SuppressFinalize(this);
		}

		private void HandleStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private void HandleStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnEndRender += HandleEndRender;
			pEngineEvents.OnBeginUpdate += (s, e) =>
			{
				Update();
			};
		}

		private void HandleEndRender(object? sender, UpdateEventArgs e)
		{
			foreach (var wnd in Windows)
				wnd.Update();
		}

		private void HandleAsyncRender(object? sender, EventArgs e)
		{
		}

		public IWindowManager CloseAllWindows()
		{
			foreach(var window in Windows)
			{
				window.Close();
			}
			return this;
		}

		public IWindowManager Update()
		{
			Application.DoEvents();
			int closedWindows = 0;
			foreach(var window in pWindows)
			{
				if (window.IsClosed)
					closedWindows++;
			}

			if (closedWindows == pWindows.Count)
				pEngine.Stop();
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
