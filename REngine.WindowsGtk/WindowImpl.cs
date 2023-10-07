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

		public override Size Size 
		{ 
			get
			{
				GetWindow().GetSize(out int width, out int height);
				return new Size(width, height);
			}
			set
			{
				GetWindow().Resize(value.Width, value.Height);
			}
		}

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
			if (HandleResize())
				EmitResize(Size);
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

		private bool HandleResize()
		{
			if (pDisposed)
				return false;
			GetWindow().GetSize(out int width, out int height);

			int currWidth = width;
			int currHeight = height;

			width = Math.Clamp(currWidth, pMinSize.Width, pMinSize.Height);
			height = Math.Clamp(currHeight, pMinSize.Height, pMaxSize.Height);

			if(currWidth != width || currHeight != height)
			{
				GetWindow().Resize(width, height);
				return true;
			}

			return false;
		}
	}
}
