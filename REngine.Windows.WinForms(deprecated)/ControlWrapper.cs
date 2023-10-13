using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace REngine.Windows
{

	internal class ControlWrapper : IWindow
	{
		private Control? pControl;

		public event WindowEvent? OnUpdate;
		public event WindowEvent? OnShow;
		public event WindowEvent? OnClose;
		public event WindowInputEvent? OnKeyDown;
		public event WindowInputEvent? OnKeyUp;
		public event WindowResizeEvent? OnResize;

		public string Title
		{
			get => pControl?.Text ?? string.Empty;
			set
			{
				if (pControl != null)
					pControl.Text = value;
			}
		}
		public IntPtr Handle => pControl?.Handle ?? IntPtr.Zero;
		public Rectangle Bounds
		{
			get => pControl?.Bounds ?? new Rectangle();
			set
			{
				if (pControl != null)
					pControl.Bounds = value;
			}
		}
		public Size Size
		{
			get => pControl?.Size ?? new Size();
			set
			{
				if (pControl != null)
					pControl.Size = value;
			}
		}
		public Point Position
		{
			get => new Point(pControl?.Left ?? 0, pControl?.Top ?? 0);
			set
			{
				if (pControl != null)
				{
					pControl.Left = (int)value.X;
					pControl.Top = (int)value.Y;
				}
			}
		}
		public Size MinSize
		{
			get => pControl?.MinimumSize ?? new Size();
			set
			{
				if (pControl != null)
					pControl.MinimumSize = value;
			}
		}
		public Size MaxSize
		{
			get => pControl?.MaximumSize ?? new Size();
			set
			{
				if (pControl != null)
					pControl.MaximumSize = value;
			}
		}

		public bool Focused { get => pControl?.Focused ?? false; }
		public bool IsClosed { get => pControl?.IsDisposed ?? true; }

		public ControlWrapper(Control control)
		{
			pControl = control;
			pControl.KeyDown += HandleKeyDown;
			pControl.KeyUp += HandleKeyUp;
			pControl.Paint += HandlePaint;
			pControl.Resize += HandleResize;
			if((control is Form))
				((Form)control).FormClosed += HandleClose;
		}

		private void HandleClose(object? sender, FormClosedEventArgs e)
		{
			OnClose?.Invoke(this, new WindowEventArgs(sender, Handle));
		}

		private void HandleResize(object? sender, EventArgs e)
		{
			OnResize?.Invoke(this, new WindowResizeEventArgs(Size, sender, Handle));
		}

		private void HandlePaint(object? sender, PaintEventArgs e)
		{
			OnUpdate?.Invoke(this, new WindowEventArgs(sender, Handle));
		}

		private void HandleKeyDown(object? sender, KeyEventArgs e)
		{
			OnKeyDown?.Invoke(this, new WindowInputEventArgs((Core.IO.Keys)e.KeyCode, sender, Handle));
		}

		private void HandleKeyUp(object? sender, KeyEventArgs e)
		{
			OnKeyUp?.Invoke(this, new WindowInputEventArgs((Core.IO.Keys)e.KeyCode, sender, Handle));
		}

		public void Dispose()
		{
			pControl?.Dispose();
			pControl = null;
		}

		public IWindow Close()
		{
			if (pControl is Form)
				((Form)pControl).Close();
			else
				pControl?.Hide();
			return this;
		}

		public IWindow Show()
		{
			pControl?.Show();
			OnShow?.Invoke(this, new WindowEventArgs(pControl, Handle));
			return this;
		}

		public IWindow Focus()
		{
			pControl?.Focus();
			return this;
		}

		public IWindow Update()
		{
			// Vulkan has issues if invalidate is not called
			pControl?.Invalidate(new Rectangle(0, 0, 1, 1));
			return this;
		}

		public IWindow Fullscreen()
		{
			return this;
		}

		public IWindow GetNativeWindow(out Core.NativeWindow window)
		{
			throw new NotImplementedException();
		}
	}
}
