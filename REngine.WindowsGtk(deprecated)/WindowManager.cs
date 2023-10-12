using Gtk;
using REngine.Core;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	public sealed class WindowManager : IWindowManager
	{
		private readonly IEngine pEngine;
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pPipeline;
		private readonly List<WidgetImpl> pWindows = new();

		private readonly Application pApp;

		private bool pDisposed = false;

		public IReadOnlyList<IWindow> Windows => pWindows.AsReadOnly();

		public WindowManager(
			EngineEvents events,
			IEngine engine,
			IExecutionPipeline pipeline
		)
		{
			pEngine = engine;
			pEngineEvents = events;
			pPipeline = pipeline;


			events.OnStart += HandleEngineStart;
			events.OnStop += HandleEngineStop;

			Application.Init();
			pApp = new("rengine.app", GLib.ApplicationFlags.None);
			pApp.Register(GLib.Cancellable.Current);
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			pPipeline
				.AddEvent(DefaultEvents.WindowsUpdateId, (_) => Update());
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			CloseAllWindows();

			foreach (var window in Windows)
				window.Dispose();
			pWindows.Clear();

			pEngineEvents.OnStop -= HandleEngineStop;

			pApp.Dispose();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		public IWindowManager CloseAllWindows()
		{
			pWindows.ForEach(x => x.Close());
			return this;
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			WidgetImpl window;

			if(createInfo.WindowInstance != null)
			{
				if (createInfo.WindowInstance is Gtk.Window wnd)
					window = new WindowImpl(wnd);
				else if (createInfo.WindowInstance is Gtk.Widget widget)
					window = new WidgetImpl(widget);
				else
					throw new InvalidCastException("WindowInstance is not Gtk type compatible.");
			}
			else
			{
				Gtk.Window wnd = new Gtk.Window(WindowType.Toplevel);
				window = new WindowImpl(wnd);
			}

			window.Title = createInfo.Title;
			window.Size = createInfo.Size;

			if (createInfo.Position != null)
				window.Position = createInfo.Position.Value;

			pWindows.Add(window);
			return window;
		}

		private void Update()
		{
			if (pDisposed)
				return;

			while (Application.EventsPending())
				Application.RunIteration();

			int closedWindows = 0;
			foreach (var window in pWindows)
			{
				if (window.IsClosed)
					closedWindows++;
				else
					window.Update();
			}

			if (closedWindows == pWindows.Count)
				pEngine.Stop();
		}

		public static Gtk.Widget GetWidget(IWindow window)
		{
			WidgetImpl? widgetImpl = window as WidgetImpl;
			if (widgetImpl is null)
				throw new InvalidCastException("IWindow was not created by this WindowManager.");
			return widgetImpl.GetWidget();
		}
	}
}
