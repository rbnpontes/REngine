using Gtk;
using REngine.Sandbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal partial class SampleWindow
	{
		protected readonly SamplesListStore pStore = new SamplesListStore();
		protected readonly TreeView pSamplesList = new TreeView();
		protected readonly Window pGameContent = new Window(WindowType.Toplevel);

		public Window GameContentWindow { get => pGameContent; }

		private void InitializeComponents()
		{
			Resize(800, 500);
			WindowPosition = WindowPosition.Center;
			Title = pGameContent.Title = "[REngine] Sandbox";
			Name = "REngine.Sandbox";

			Box vpanel = new (Orientation.Vertical, 5);
			vpanel.Expand = false;
			vpanel.WidthRequest = 200;

			pSamplesList.Expand = true;
			pSamplesList.AppendColumn(
				new TreeViewColumn("Sample Name", new CellRendererText(), "text", 0, null)
			);
			pSamplesList.Model = pStore;
			pSamplesList.EnableSearch = false;

			Button loadButton = new() { Label = "Load Sample" };
			loadButton.Clicked += (s, e) => ExecClickLoadSample();

			vpanel.Add(pSamplesList);
			vpanel.Add(loadButton);

			Gtk.Box hpanel = new (Gtk.Orientation.Horizontal, 5);

			Viewport viewport = new();
			viewport.Expand = true;

			hpanel.Add(vpanel);
			hpanel.Add(viewport);

			Add(hpanel);
			
			ShowAll();

			viewport.Child = pGameContent;
			pGameContent.ShowAll();

			KeyPressEvent += HandleKeyPress;
			KeyReleaseEvent += HandleKeyRelease;

			AddEvents((int)(Gdk.EventMask.KeyPressMask | Gdk.EventMask.KeyReleaseMask));
		}

		private void HandleKeyRelease(object o, KeyReleaseEventArgs args)
		{
			OnKeyUp(args.Event.Key);
			args.RetVal = false;
		}

		private void HandleKeyPress(object o, KeyPressEventArgs args)
		{
			OnKeyDown(args.Event.Key);
			OnInput(args.Event.KeyValue);
			args.RetVal = false;
		}

		private void ExecClickLoadSample()
		{
			SampleItem? item = SamplesListStore.GetSelected(pStore, pSamplesList.Selection);
			if (item != null)
				LoadSample(item);
		}
	}
}
