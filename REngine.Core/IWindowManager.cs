using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public struct WindowCreationInfo
	{
		public string Title;
		public Size Size;
		public Point? Position;
		/// <summary>
		/// Must be a Winforms control or Gtk Widget
		/// WindowInstance must be compatible with Window System
		/// otherwise, window manager will thrown an exception
		/// </summary>
		public object? WindowInstance;

		public WindowCreationInfo()
		{
			Title = string.Empty;
			Size = new Size(100, 100);
			Position = null;
			WindowInstance = null;
		}
	}

	public interface IWindowManager : IDisposable
	{
		public IReadOnlyList<IWindow> Windows { get; }
		public IWindowManager CloseAllWindows();
		public IWindow Create(WindowCreationInfo createInfo);
	}
}
