using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class WindowEventArgs : EventArgs
	{
		public object WindowObject { get; set; }
		public IntPtr Handle { get; set; }
		public WindowEventArgs(object windowObj, IntPtr handle) 
		{
			WindowObject = windowObj;
			Handle = handle;
		}
	}

	public class WindowResizeEventArgs : WindowEventArgs
	{
		public Size Size { get; private set; }
		public WindowResizeEventArgs(Size newSize, object windowObj, IntPtr handle) : base(windowObj, handle)
		{
			Size = newSize;
		}
	}

	public class WindowInputEventArgs : WindowEventArgs 
	{
		public Keys Keys { get; private set; }
		public WindowInputEventArgs(Keys keys, object windowObj, IntPtr handle) : base(windowObj, handle)
		{
			Keys = keys;
		}
	}

	public delegate void WindowEvent(object sender, WindowEventArgs e);
	public delegate void WindowResizeEvent(object sender, WindowResizeEventArgs e);
	public delegate void WindowInputEvent(object sender, WindowInputEventArgs e);

	public interface IWindow : IDisposable
	{
		public event WindowEvent? OnUpdate;
		public event WindowEvent? OnShow;
		public event WindowEvent? OnClose;
		public event WindowInputEvent? OnKeyDown;
		public event WindowInputEvent? OnKeyUp;
		public event WindowResizeEvent? OnResize;

		public string Title { get; set; }
		public IntPtr Handle { get; }
		public Rectangle Bounds { get; set; }
		public Size Size { get; set; }
		public Point Position { get; set; }
		public Size MinSize { get; set; }
		public Size MaxSize { get; set; }
		public bool Focused { get; }
		public bool IsClosed { get; }

		public IWindow Close();
		public IWindow Show();
		public IWindow Focus();

		public IWindow Update();

		public IWindow Fullscreen();

		public IWindow GetNativeWindow(out NativeWindow window);
	}
}
