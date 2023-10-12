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
		public InputKey Key { get; private set; }
		public WindowInputEventArgs(InputKey key, object windowObj, IntPtr handle) : base(windowObj, handle)
		{
			Key = key;
		}
	}

	public class WindowInputTextEventArgs : WindowEventArgs 
	{ 
		public string Value { get; private set; }
		public WindowInputTextEventArgs(string value, object windowObj, IntPtr handle) : base(windowObj, handle)
		{
			Value = value;
		}
	}

	public class WindowMouseEventArgs : WindowEventArgs
	{
		public MouseKey MouseKey { get; private set; }
		public Vector2 Position { get; set; }
		public WindowMouseEventArgs(MouseKey mouseKey, object windowObj, IntPtr handle) : base(windowObj, handle)
		{
			MouseKey = mouseKey;
		}
	}

	public delegate void WindowEvent(object sender, WindowEventArgs e);
	public delegate void WindowResizeEvent(object sender, WindowResizeEventArgs e);
	public delegate void WindowInputEvent(object sender, WindowInputEventArgs e);
	public delegate void WindowInputTextEvent(object sender, WindowInputTextEventArgs e);
	public delegate void WindowMouseEvent(object sender, WindowMouseEventArgs e);

	public interface IWindow : IDisposable
	{
		public event WindowEvent? OnUpdate;
		public event WindowEvent? OnShow;
		public event WindowEvent? OnClose;
		public event WindowInputEvent? OnKeyDown;
		public event WindowInputEvent? OnKeyUp;
		public event WindowInputTextEvent? OnInput;
		public event WindowResizeEvent? OnResize;
		public event WindowMouseEvent? OnMouseDown;
		public event WindowMouseEvent? OnMouseUp;
		public event WindowMouseEvent? OnMouseMove;

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
		public IWindow ExitFullscreen();

		public IWindow GetNativeWindow(out NativeWindow window);

		public IWindow ForwardKeyDownEvent(InputKey key);
		public IWindow ForwardKeyUpEvent(InputKey key);
		public IWindow ForwardInputEvent(int utf32Char);
	}
}
