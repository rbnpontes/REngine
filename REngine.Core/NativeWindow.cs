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
	}
#endif
}
