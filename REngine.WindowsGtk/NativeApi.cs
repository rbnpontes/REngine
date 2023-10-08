using Gtk;
using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal static class NativeApi
	{
#if LINUX
		private const string WindowX11Name = "GdkX11Window";
		private const string WindowWaylandName = "GdkWaylandWindow";
#endif

#if WINDOWS
		[DllImport("libgdk-3-0.dll")]
		private static extern IntPtr gdk_win32_window_get_handle(IntPtr gdkWindow);
#endif
#if LINUX
		[DllImport("libgdk-3.so.0")]
		private static extern IntPtr gdk_wayland_display_get_wl_display(IntPtr gdkDisplay);

		[DllImport("libgdk-3.so.0")]
		private static extern uint gdk_x11_window_get_xid(IntPtr gdkWindow);

		[DllImport("libgdk-3.so.0")]
		private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr gdkDisplay);
#endif
		private static void GetNativeWindow(Gdk.Window window, out NativeWindow output)
		{
#if WINDOWS
			output = new NativeWindow { Hwnd = gdk_win32_window_get_handle(window.Handle) };
#elif LINUX
			string a = window.NativeType.ToString();
			if (string.Equals(a, WindowX11Name))
			{
				IntPtr display = gdk_x11_display_get_xdisplay(window.Display.Handle);
				output = new NativeWindow
				{
					Display = display,
					WindowId = gdk_x11_window_get_xid(window.Handle)
				};
			}
			else 
			{
				throw new NotSupportedException("Unfortunately, only X11 Windows Manager is supported. If you are under Linux and have Xorg Windows Manager, we recomend to use this instead.");
			}
#else
			throw new NotImplementedException();
#endif
		}

		public static void GetNativeWindow(Gtk.Widget widget, out NativeWindow output)
		{
			GetNativeWindow(widget.Window, out output);
		}
	}
}
