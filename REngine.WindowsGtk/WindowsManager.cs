using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	public sealed class WindowsManager : IWindowManager
	{
		private readonly List<WindowImpl> pWindows = new List<WindowImpl>();

		public IReadOnlyList<IWindow> Windows => pWindows.AsReadOnly();

		public IWindowManager CloseAllWindows()
		{
			pWindows.ForEach(x => x.Close());
			return this;
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			if(createInfo.WindowInstance != null)
			{

			}
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public static Gtk.Widget GetWidget(IWindow window)
		{
			WidgetImpl? widgetImpl = window as WidgetImpl;
			if (widgetImpl is null)
				throw new InvalidCastException("IWindow was not created by this WindowsManager.");
			return widgetImpl.GetWidget();
		}
	}
}
