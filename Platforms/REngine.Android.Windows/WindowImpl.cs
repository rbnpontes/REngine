using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using REngine.Core;
using REngine.Core.IO;
using Point = System.Drawing.Point;

namespace REngine.Android.Windows
{
	internal class WindowImpl : IWindow
	{
		private readonly SurfaceView pSurfaceView;
		private readonly SurfaceCallback pCallback;

		private Rectangle pBounds;
		private bool pDisposed;

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
		public event WindowMouseWheelEvent? OnMouseWheel;

		public string Title
		{
			get => string.Empty;
			set {}
		}
		public IntPtr Handle => pCallback.Handle;

		public Rectangle Bounds
		{
			get => pBounds;
			set {}
		}

		public Size Size
		{
			get => pBounds.Size;
			set {}
		}

		public Point Position
		{
			get => Point.Empty;
			set {}
		}
		public Size MinSize { get; set; }
		public Size MaxSize { get; set; }
		public bool Focused => true;
		public bool IsClosed => false;
		public bool IsMinimized => false;
		public bool IsFullscreen => false;

		public WindowImpl(SurfaceView surfaceView, SurfaceCallback callback)
		{
			pSurfaceView = surfaceView;
			pCallback = callback;
			pSurfaceView.Holder?.AddCallback(pCallback);
			UpdateBounds();
		}

		public void Dispose()
		{
			if(pDisposed) return;

			pDisposed = true;
		}

		public IWindow Close()
		{
			return this;
		}

		public IWindow Show()
		{
			return this;
		}

		public IWindow Focus()
		{
			return this;
		}

		public IWindow Update()
		{
			UpdateBounds();
			return this;
		}

		public IWindow Fullscreen()
		{
			return this;
		}

		public IWindow ExitFullscreen()
		{
			return this;
		}

		public IWindow GetNativeWindow(out NativeWindow window)
		{
			window = new NativeWindow() { AndroidNativeWindow = pCallback.NativeWindow };
			return this;
		}

		public IWindow ForwardKeyDownEvent(InputKey key)
		{
			return this;
		}

		public IWindow ForwardKeyUpEvent(InputKey key)
		{
			return this;
		}

		public IWindow ForwardInputEvent(int utf32Char)
		{
			return this;
		}

		private void UpdateBounds()
		{
			pBounds = new Rectangle(
				pSurfaceView.Left,
				pSurfaceView.Top,
				pSurfaceView.Width,
				pSurfaceView.Height
			);
		}
	}
}
