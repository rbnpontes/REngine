using REngine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal class WindowImpl : IWindow
	{
		private readonly Gtk.Window pWindow;

		private bool pDisposed;

		public string Title 
		{ 
			get
			{
				if (pDisposed) 
					return string.Empty;
				return pWindow.Title;
			}
			set
			{
				if (pDisposed)
					return;
				pWindow.Title = value;
			}
		}

		public IntPtr Handle => pWindow.Handle;

		public Rectangle Bounds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Size Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Vector2 Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Size MinSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Size MaxSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool Focused => pWindow.IsFocus;

		public bool IsClosed => pWindow.Visible;

		public event WindowEvent? OnUpdate;
		public event WindowEvent? OnShow;
		public event WindowEvent? OnClose;
		public event WindowInputEvent? OnKeyDown;
		public event WindowInputEvent? OnKeyUp;
		public event WindowResizeEvent? OnResize;

		public WindowImpl(Gtk.Window window)
		{

		}

		public IWindow Close()
		{
			pWindow.Close();
			return this;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pWindow.Close();
			pWindow.Dispose();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		public IWindow Focus()
		{
			throw new NotImplementedException();
		}

		public IWindow Show()
		{
			throw new NotImplementedException();
		}

		public IWindow Update()
		{
			throw new NotImplementedException();
		}
	}
}
