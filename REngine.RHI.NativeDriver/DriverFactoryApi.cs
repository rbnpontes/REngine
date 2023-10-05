using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	public partial class DriverFactory
	{
		delegate void MessageEventCallback(DbgMsgSeverity severity, IntPtr msg, IntPtr func, IntPtr file, int line);
		[DllImport(Constants.Lib)]
		static extern void rengine_get_available_adapter(GraphicsBackend backend, IntPtr messageEvent, ref ResultNative result, ref uint length);
		[DllImport(Constants.Lib)]
		static extern void rengine_create_driver(ref DriverSettingsNative settings, IntPtr swapChainDesc, [In] in NativeWindow nativeWindow, ref DriverResult result);
	}
}
