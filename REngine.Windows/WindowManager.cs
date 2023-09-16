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

		public IReadOnlyList<IWindow> Windows { get => pWindows.AsReadOnly(); }

		public WindowManager(EngineEvents events)
		{
			events.OnEndUpdate += HandleEndUpdate;
			events.OnEndRender += HandleEndRender;
			events.OnStop += (s, e) => Dispose();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
		}

		private void HandleEndUpdate(object sender, UpdateEventArgs args)
		{
			Update();
		}

		private void HandleEndRender(object sender, UpdateEventArgs args)
		{
			foreach (var wnd in Windows)
				wnd.Update();
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
			return this;
		}

		public void Dispose()
		{
			CloseAllWindows();
			foreach(var window in Windows)
				window.Dispose();
			pWindows.Clear();
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
			}

			window.Title = createInfo.Title;
			window.Size = createInfo.Size;

			if (createInfo.Position != null)
				window.Position = createInfo.Position.Value;

			pWindows.Add(window);
			return window;
		}
	}
}
