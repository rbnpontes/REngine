using Gtk;
using REngine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal class WidgetImpl : IWindow
	{
		protected readonly Gtk.Widget pWidget;
		protected readonly WindowEventArgs pDefaultEventArgs;

		protected bool pDisposed;

		public virtual string Title 
		{ 
			get => string.Empty;
			set { } 
		}

		public IntPtr Handle => pWidget.Handle;

		public virtual Rectangle Bounds { get => new Rectangle(); set { } }

		public virtual Size Size { get => new Size(); set { } }
		public virtual Point Position { get => new Point(); set { } }

		public virtual Size MinSize { get => new Size(); set { } }
		public virtual Size MaxSize { get => new Size(); set { } }

		public virtual bool Focused => pWidget.HasFocus;

		public bool IsClosed => !pWidget.IsVisible;

		public event WindowEvent? OnUpdate;
		public event WindowEvent? OnShow;
		public event WindowEvent? OnClose;
		public event WindowInputEvent? OnKeyDown;
		public event WindowInputEvent? OnKeyUp;
		public event WindowResizeEvent? OnResize;

		public WidgetImpl(Gtk.Widget widget)
		{
			pWidget = widget;
			pDefaultEventArgs = new WindowEventArgs(widget, widget.Handle);

		}

		public virtual IWindow Close()
		{
			pWidget.Hide();
			return this;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			OnDispose();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		protected virtual void OnDispose()
		{
			pWidget.Dispose();
		}

		public virtual IWindow Focus()
		{
			return this;
		}

		public virtual IWindow Fullscreen()
		{
			return this;
		}

		public IWindow GetNativeWindow(out NativeWindow window)
		{
			NativeApi.GetNativeWindow(pWidget, out window);
			return this;
		}

		public IWindow Show()
		{
			pWidget.ShowAll();
			OnShow?.Invoke(this, pDefaultEventArgs);
			return this;
		}

		public virtual IWindow Update()
		{
			OnUpdate?.Invoke(this, pDefaultEventArgs);
			return this;
		}

		protected void EmitResize(Size size)
		{
			OnResize?.Invoke(this, new WindowResizeEventArgs(size, pWidget, Handle));
		}

		public Widget GetWidget() { return pWidget; }
	}
}
