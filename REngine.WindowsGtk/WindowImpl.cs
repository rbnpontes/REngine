using GLib;
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
	internal class WindowImpl : WidgetImpl
	{
		public override string Title 
		{ 
			get
			{
				if (pDisposed) 
					return string.Empty;
				return GetWindow().Title;
			}
			set
			{
				if (pDisposed)
					return;
				GetWindow().Title = value;
			}
		}

		public override Rectangle Bounds 
		{ 
			get
			{
				GetWindow().GetSize(out int width, out int height);
				GetWindow().GetPosition(out int x, out int y);
				return new Rectangle(x, y, width, height);
			}
			set
			{
				GetWindow().Move(value.X, value.Y);
				GetWindow().Resize(value.Width, value.Height);
			}
		}

		public override Size Size { get; set; }
		public override Point Position 
		{
			get
			{
				GetWindow().GetPosition(out int x, out int y);
				return new Point(x, y);
			}
			set
			{
				GetWindow().Move(value.X, value.Y);
			}
		}

		private Size pMinSize = new();
		private Size pMaxSize = new(int.MaxValue, int.MaxValue);

		public override Size MinSize 
		{
			get => pMinSize;
			set => pMinSize = value;
		}
		public override Size MaxSize 
		{
			get => pMaxSize;
			set => pMaxSize = value;
		}

		public WindowImpl(Gtk.Window window) : base(window)
		{
			window.ConfigureEvent += HandleConfigure;
		}

		private void HandleConfigure(object o, Gtk.ConfigureEventArgs args)
		{
			GetWindow().GetSize(out int w, out int h);
			Size = new Size(w, h);
		}

		private Gtk.Window GetWindow()
		{
			Gtk.Window? window = pWidget as Gtk.Window;
			if (window is null)
				throw new NullReferenceException("pWidget does not inherit from Gtk.Window");
			return window;
		}

		public override IWindow Close()
		{
			GetWindow().Close();
			return this;
		}

		protected override void OnDispose()
		{
			GetWindow().Close();
		}

		public override IWindow Focus()
		{
			GetWindow().Present();
			return this;
		}

		public override IWindow Fullscreen()
		{
			GetWindow().Fullscreen();
			if (HandleResize())
				EmitResize(Size);
			return this;
		}

		public override IWindow Update()
		{
			HandleResize();
			return this;
		}

		private bool HandleResize()
		{
			if (pDisposed)
				return false;
			GetWindow().GetSize(out int width, out int height);

			width = Math.Clamp(width, pMinSize.Width, pMaxSize.Width);
			height = Math.Clamp(height, pMinSize.Height, pMaxSize.Height);

			if(Size.Width != width || Size.Height != height)
			{
				Size = new Size(width, height);
				EmitResize(Size);
				return true;
			}

			return false;
		}
	}
}
