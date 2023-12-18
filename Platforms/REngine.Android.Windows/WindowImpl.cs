using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
		private readonly GameView pGameView;

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
		public IntPtr Handle => pGameView.Handle;

		public Rectangle Bounds
		{
			get => pGameView.Bounds;
			set {}
		}

		public Size Size
		{
			get => pGameView.Size;
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

		public WindowImpl(GameView gameView)
		{
			pGameView = gameView;
			var touchListener = new WindowTouchListener(this);
			var keyListener = new KeyboardListener(this);
			pGameView.SetOnTouchListener(touchListener);
			pGameView.SetKeyboardListener(keyListener);
		}

		public void Dispose()
		{
			if(pDisposed) return;

			pGameView.SetCallback(null);
			pGameView.SetKeyboardListener(null);
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
			// TODO: use better approach
#if ANDROID
			window = new NativeWindow() { AndroidNativeWindow = pGameView.NativeWindow };
			return this;
#else
			throw new NotImplementedException();
#endif
		}

		public IWindow ForwardKeyDownEvent(InputKey key)
		{
			ForwardKeyEvent(key, true);
			return this;
		}

		public IWindow ForwardKeyUpEvent(InputKey key)
		{
			ForwardKeyEvent(key, false);
			return this;
		}

		private void ForwardKeyEvent(InputKey key, bool isDown)
		{
			var args = new WindowInputEventArgs(key, pGameView, pGameView.NativeWindow);
			if(isDown)
				OnKeyDown?.Invoke(this, args);
			else
				OnKeyUp?.Invoke(this, args);
		}
		public IWindow ForwardInputEvent(int utf32Char)
		{
			OnInput?.Invoke(this, new WindowInputTextEventArgs(
				char.ConvertFromUtf32(utf32Char), pGameView, pGameView.NativeWindow)
			);
			return this;
		}

		public IWindow ForwardMouseMove(Vector2 position)
		{
			OnMouseMove?.Invoke(this, 
				new WindowMouseEventArgs(MouseKey.None, pGameView, pGameView.NativeWindow)
				{
					Position = position
				}
			);
			return this;
		}

		public IWindow ForwardMouseDown(MouseKey mouseKey)
		{
			ForwardMouseAction(mouseKey, true);
			return this;
		}

		public IWindow ForwardMouseUp(MouseKey mouseKey)
		{
			ForwardMouseAction(mouseKey, false);
			return this;
		}

		public IWindow ForwardMouseWheel(Vector2 axis)
		{
			OnMouseWheel?.Invoke(this, new WindowMouseWheelEventArgs(axis, pGameView, pGameView.NativeWindow));
			return this;
		}
		
		private void ForwardMouseAction(MouseKey mouseKey, bool isDown)
		{
			var evt = new WindowMouseEventArgs(mouseKey, pGameView, pGameView.NativeWindow);
			(isDown ? OnMouseDown : OnMouseUp)?.Invoke(this, evt);
		}
	}
}
