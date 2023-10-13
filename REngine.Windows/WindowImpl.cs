using GLFW;
using REngine.Core;
using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	internal class WindowImpl : IWindow
	{
		const byte MaxWaitResizeFrames = 15;

		private readonly GLFW.Window pWindow;
		protected readonly WindowEventArgs pDefaultEventArgs;
		private readonly SizeCallback pResizeCallback;
		private readonly MouseButtonCallback pMouseButtonCallback;
		private readonly MouseCallback pMouseCallback;
		private readonly MouseCallback pMouseWheelCallback;
		private readonly KeyCallback pKeyboardCallback;
		private readonly CharCallback pInputCallback;

		private bool pDisposed = false;
		private string pTitle;

		public string Title 
		{ 
			get => pTitle; 
			set => Glfw.SetWindowTitle(pWindow, pTitle = value);
		}

		public IntPtr Handle => IntPtr.Zero;

		public Rectangle Bounds 
		{ 
			get
			{
				Rectangle bounds = new();
				Glfw.GetWindowPosition(pWindow, out int x, out int y);
				Glfw.GetWindowSize(pWindow, out int w, out int h);

				bounds.X = x;
				bounds.Y = y;
				bounds.Width = w;
				bounds.Height = h;
				return bounds;
			}
			set
			{
				Glfw.SetWindowPosition(pWindow, value.X, value.Y);
				Glfw.SetWindowSize(pWindow, value.Width, value.Height);
			}
		}
		public Size Size 
		{
			get
			{
				Glfw.GetWindowSize(pWindow, out int w, out int h);
				return new Size(w, h);
			}
			set => Glfw.SetWindowSize(pWindow, value.Width, value.Height);
		}
		public Point Position 
		{
			get 
			{
				Glfw.GetWindowPosition(pWindow, out int x, out int y);
				return new Point(x, y);
			}
			set => Glfw.SetWindowPosition(pWindow, value.X, value.Y);
		}

		private Size pMinSize = new Size();
		private Size pMaxSize = new Size(ushort.MaxValue, ushort.MaxValue);

		public Size MinSize 
		{
			get => pMinSize;
			set
			{
				pMinSize = value;
				Glfw.SetWindowSizeLimits(pWindow, value.Width, value.Height, pMaxSize.Width, pMaxSize.Height);
			}
		}
		public Size MaxSize 
		{
			get => pMaxSize;
			set
			{
				pMaxSize = value;
				Glfw.SetWindowSizeLimits(pWindow, pMinSize.Width, pMinSize.Height, value.Width, value.Height);
			}
		}

		public bool Focused { get; private set; }

		public bool IsClosed 
		{ 
			get => Glfw.WindowShouldClose(pWindow);
		}

		private bool pFullscreen = false;
		public bool IsFullscreen
		{
			get => pFullscreen;
		}

		private byte pResizeDirtyCount = 0;
		private bool pDirtyResize = false;
		private Size pNewSize = new();

		private Rectangle pWindowedBounds = Rectangle.Empty;

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

		public WindowImpl(GLFW.Window window, string title)
		{
			pWindow = window;
			pTitle = title;
			pDefaultEventArgs = new(window, IntPtr.Zero);

			pResizeCallback = HandleWindowSize;
			pMouseButtonCallback = HandleMouseButton;
			pMouseCallback = HandleMouseMove;
			pMouseWheelCallback = HandleMouseWheel;
			pKeyboardCallback = HandleKeyboard;
			pInputCallback = HandleInput;


			Glfw.SetWindowSizeCallback(window, pResizeCallback);
			Glfw.SetMouseButtonCallback(window, pMouseButtonCallback);
			Glfw.SetCursorPositionCallback(window, pMouseCallback);
			Glfw.SetScrollCallback(window, pMouseWheelCallback);
			Glfw.SetKeyCallback(window, pKeyboardCallback);
			Glfw.SetCharCallback(window, pInputCallback);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			Close();
			Glfw.DestroyWindow(pWindow);

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		public IWindow Close()
		{
			Glfw.SetWindowShouldClose(pWindow, true);
			return this;
		}

		public IWindow Focus()
		{
			Glfw.FocusWindow(pWindow);
			return this;
		}

		public IWindow ForwardInputEvent(int utf32Char)
		{
			throw new NotImplementedException();
		}

		public IWindow ForwardKeyDownEvent(InputKey key)
		{
			throw new NotImplementedException();
		}

		public IWindow ForwardKeyUpEvent(InputKey key)
		{
			throw new NotImplementedException();
		}

		private GLFW.Monitor GetPreferredMonitor()
		{
			var monitors = Glfw.Monitors;
			var wndBounds = Bounds;
			for(int i =0; i <  monitors.Length; i++)
			{
				if (wndBounds.IntersectsWith(monitors[i].WorkArea))
					return monitors[i];
			}

			return Glfw.PrimaryMonitor;
		}
		private GLFW.VideoMode GetBestVideoMode(GLFW.Monitor monitor)
		{
			var videoModes =  Glfw.GetVideoModes(monitor);
			var filteredVideos = videoModes.Where(x =>
			{
				return x.Width == monitor.WorkArea.Width && x.Height == monitor.WorkArea.Height;
			});

			if (filteredVideos.Count() == 0)
				return videoModes[0];
			return filteredVideos.FirstOrDefault();
		}
		
		public IWindow Fullscreen()
		{
			if (pFullscreen)
				return this;
			pWindowedBounds = Bounds;
			var monitor = GetPreferredMonitor();

			Glfw.SetWindowMonitor(
				pWindow, monitor,
				0, 0,
				monitor.WorkArea.Width,
				monitor.WorkArea.Height,
				GetBestVideoMode(monitor).RefreshRate
			);
			pFullscreen = true;
			return this;
		}

		public IWindow ExitFullscreen()
		{
			if (!pFullscreen)
				return this;

			Glfw.SetWindowMonitor(
				pWindow,
				GLFW.Monitor.None,
				pWindowedBounds.X,
				pWindowedBounds.Y,
				pWindowedBounds.Width,
				pWindowedBounds.Height,
				0
			);
			pFullscreen = false;
			return this;
		}

		public IWindow GetNativeWindow(out Core.NativeWindow window)
		{
			GlfwUtils.GetNativeWindow(pWindow, out window);
			return this;
		}

		public IWindow Show()
		{
			Glfw.ShowWindow(pWindow);
			OnShow?.Invoke(this, pDefaultEventArgs);
			return this;
		}

		public IWindow Update()
		{
			if (pDirtyResize)
			{
				if (pResizeDirtyCount > MaxWaitResizeFrames)
				{
					OnResize?.Invoke(this, new WindowResizeEventArgs(new Size(pNewSize.Width, pNewSize.Height), pWindow, IntPtr.Zero));
					pDirtyResize = false;
				}
				++pResizeDirtyCount;
			}
			return this;
		}
	
		private void HandleWindowSize(GLFW.Window window, int width, int height)
		{
			pNewSize = new Size(width, height);
			pResizeDirtyCount = 0;
			pDirtyResize = true;
		}

		private void HandleMouseButton(GLFW.Window window, MouseButton button, InputState state, ModifierKeys modifiers)
		{
			MouseKey key = InputConverter.GetMouseKey(button);
			var evt = new WindowMouseEventArgs(key, pWindow, IntPtr.Zero);

			(state == InputState.Press ? OnMouseDown : OnMouseUp)?.Invoke(this, evt);
		}

		private void HandleMouseMove(GLFW.Window window, double x, double y)
		{
			OnMouseMove?.Invoke(this, new WindowMouseEventArgs(MouseKey.None, pWindow, IntPtr.Zero)
			{
				Position = new Vector2((float)x, (float)y)
			});
		}

		private void HandleMouseWheel(GLFW.Window window, double x, double y)
		{
			OnMouseWheel?.Invoke(this,
				new WindowMouseWheelEventArgs(new Vector2((float)x, (float)y), pWindow, IntPtr.Zero)
			);
		}

		private void HandleKeyboard(GLFW.Window window, GLFW.Keys key, int scanCode, InputState state, ModifierKeys modifiers)
		{
			EmitKeyboard(InputConverter.GetInputKey(key), state);	
		}

		private void EmitKeyboard(InputKey key, InputState state)
		{
			(state == InputState.Press ? OnKeyDown : OnKeyUp)?.Invoke(this, new WindowInputEventArgs(key, pWindow, IntPtr.Zero));
		}

		private void HandleInput(GLFW.Window window, uint codePoint)
		{
			OnInput?.Invoke(this, 
				new WindowInputTextEventArgs(char.ConvertFromUtf32((int)codePoint), pWindow, IntPtr.Zero)
			);
		}
	}
}
