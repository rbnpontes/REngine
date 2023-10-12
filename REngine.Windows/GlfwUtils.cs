using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	internal static class GlfwUtils
	{
		public static void GetNativeWindow(in GLFW.Window window, out NativeWindow nativeWindow)
		{
#if WINDOWS
			nativeWindow = new NativeWindow
			{
				Hwnd = GLFW.Native.GetWin32Window(window),
			};
#elif LINUX
			string? env = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
			if (string.IsNullOrEmpty(env))
				throw new Exception("Can´t detect window system.");

			if(string.Equals(env, "wayland"))
			{
				nativeWindow = new NativeWindow
				{
					Display = GLFW.Native.GetWaylandDisplay(),
					WindowId = (uint)GLFW.Native.GetWaylandWindow(window)
				};
			}
			else if(string.Equals(env, "x11"))
			{
				nativeWindow = new NativeWindow
				{
					Display = GLFW.Native.GetX11Display(),
					WindowId = (uint)GLFW.Native.GetX11Window(window)
				};
			}
			else
			{
				throw new NotImplementedException($"Not implemented window type {env}");
			}
#else
			throw new NotImplementedException();
#endif
		}
	}
}
