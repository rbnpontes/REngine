using REngine.Core;
using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal class WidgetImpl : IWindow
	{
		protected readonly Gtk.Widget pWidget;
		protected readonly WindowEventArgs pDefaultEventArgs;

		private Vector2 pMousePosition;
		private bool pIsMouseInside = false;

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
		public event WindowInputTextEvent? OnInput;
		public event WindowResizeEvent? OnResize;
		public event WindowMouseEvent? OnMouseDown;
		public event WindowMouseEvent? OnMouseUp;
		public event WindowMouseEvent? OnMouseMove;

		public WidgetImpl(Gtk.Widget widget)
		{
			pWidget = widget;
			pDefaultEventArgs = new WindowEventArgs(widget, widget.Handle);

			pWidget.ButtonPressEvent += HandleButtonPress;
			pWidget.ButtonReleaseEvent += HandleButtonRelease;

			pWidget.KeyPressEvent += HandleKeyPress;
			pWidget.KeyReleaseEvent += HandleKeyRelease;

			pWidget.MotionNotifyEvent += HandleMotionNotify;

			pWidget.CanFocus = true;

			pWidget.AddEvents((int)(Gdk.EventMask.KeyPressMask | Gdk.EventMask.KeyReleaseMask | Gdk.EventMask.PointerMotionMask));
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pWidget.KeyPressEvent -= HandleKeyPress;
			pWidget.KeyReleaseEvent -= HandleKeyRelease;

			pWidget.ButtonPressEvent -= HandleButtonPress;
			pWidget.ButtonReleaseEvent -= HandleButtonRelease;

			pWidget.MotionNotifyEvent -= HandleMotionNotify;

			OnDispose();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void HandleMotionNotify(object o, Gtk.MotionNotifyEventArgs args)
		{
			pWidget.Display.GetPointer(out int x, out int y);
			pWidget.Window.GetOrigin(out int originX, out int originY);

			Vector2 msPos = new Vector2(x - originX, y - originY);

			if (msPos == pMousePosition)
				return;
			pMousePosition = msPos;

			EmitMouseMove(MouseKey.None);
		}

		private void HandleKeyRelease(object o, Gtk.KeyReleaseEventArgs args)
		{
			ForwardKeyUpEvent(InputConverter.GetKeys(args.Event.Key));
			args.RetVal = true;
		}

		private void HandleKeyPress(object o, Gtk.KeyPressEventArgs args)
		{
			ForwardKeyDownEvent(InputConverter.GetKeys(args.Event.Key));
			ForwardInputEvent((int)args.Event.KeyValue);
			args.RetVal = true;
		}

		private uint pLastReleaseTime = 0;
		private void HandleButtonRelease(object o, Gtk.ButtonReleaseEventArgs args)
		{
			var button = args.Event.Button;
			if (args.Event.Time == pLastReleaseTime)
				return;
			pLastReleaseTime = args.Event.Time;
			EmitMouseUp(InputConverter.GetMouseKey(button));
		}


		private uint pLastPressTime = 0;
		private void HandleButtonPress(object o, Gtk.ButtonPressEventArgs args)
		{
			var button = args.Event.Button;
			if (args.Event.Time == pLastPressTime)
				return;
			pLastPressTime = args.Event.Time;
			EmitMouseDown(InputConverter.GetMouseKey(button));
		}

		public virtual IWindow Close()
		{
			pWidget.Hide();
			return this;
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

		public virtual IWindow ExitFullscreen()
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

		public virtual IWindow ForwardKeyDownEvent(InputKey key)
		{
			OnKeyDown?.Invoke(this, new WindowInputEventArgs(key, pWidget, Handle));
			return this;
		}
		public virtual IWindow ForwardKeyUpEvent(InputKey key)
		{
			OnKeyUp?.Invoke(this, new WindowInputEventArgs(key, pWidget, Handle));
			return this;
		}
		public virtual IWindow ForwardInputEvent(int utf32Char)
		{
			string value = char.ConvertFromUtf32(utf32Char);
			int utf32Code = char.ConvertToUtf32(value, 0);
			OnInput?.Invoke(this, new WindowInputTextEventArgs(value, pWidget, Handle));
			return this;
		}

		protected void EmitResize(Size size)
		{
			OnResize?.Invoke(this, new WindowResizeEventArgs(size, pWidget, Handle));
		}

		public Gtk.Widget GetWidget() { return pWidget; }

		protected void EmitMouseDown(MouseKey key)
		{
			OnMouseDown?.Invoke(this, new WindowMouseEventArgs(key, pWidget, Handle)
			{
				Position = pMousePosition
			});
		}
		protected void EmitMouseUp(MouseKey key)
		{
			OnMouseUp?.Invoke(this, new WindowMouseEventArgs(key, pWidget, Handle)
			{
				Position = pMousePosition
			});
		}
		protected void EmitMouseMove(MouseKey key)
		{
			OnMouseMove?.Invoke(this, new WindowMouseEventArgs(key, pWidget, Handle)
			{
				Position = pMousePosition
			});
		}
	}
}
