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
		public Vector2? Position;
		/// <summary>
		/// Must be a Winforms control
		/// otherwise, window manager will thrown an exception
		/// </summary>
		public object? Control;

		public WindowCreationInfo()
		{
			Title = string.Empty;
			Size = new Size(100, 100);
			Position = null;
			Control = null;
		}
	}

	public interface IWindowManager : IDisposable
	{
		public IReadOnlyList<IWindow> Windows { get; }
		public IWindowManager Update();
		public IWindowManager CloseAllWindows();
		public IWindow Create(WindowCreationInfo createInfo);
	}
}
