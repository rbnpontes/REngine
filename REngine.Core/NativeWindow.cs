using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
#if WINDOWS
	/// <summary>
	/// Native Window Handle
	/// You must fill Hwnd windows ptr
	/// </summary>
	public struct NativeWindow
	{
		public IntPtr Hwnd;

		public override string ToString()
		{
			return $"Hwnd: {Hwnd.ToString("x8")}";
		}
	}
#elif ANDROID
	public struct NativeWindow 
	{
		public IntPtr AndroidNativeWindow;
		public override string ToString()
		{
			return $"Android Native Window: {AndroidNativeWindow:x8}";
		}
	}
#elif LINUX
	/// <summary>
	/// Native Window Handle
	/// - If your window system is Wayland
	/// You only need fill Display
	/// - If your window system is XCB
	/// You must fill XCBConnection and WindowId
	/// - If your window system is XLIB
	/// You must fill Display and WindowId
	/// </summary>
	public struct NativeWindow 
	{
		public uint WindowId;
		public IntPtr Display;
		public IntPtr XCBConnection;

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("WindowId: ");
			builder.AppendLine(WindowId.ToString());

			builder.Append("Display: ");
			builder.AppendLine(Display.ToString("x8"));

			builder.Append("XCBConnection: ");
			builder.AppendLine(XCBConnection.ToString("x8"));

			return builder.ToString();
		}
	}
#endif
}
