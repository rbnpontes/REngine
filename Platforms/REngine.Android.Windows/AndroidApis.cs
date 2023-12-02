using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Android.Windows
{
	internal static class AndroidApis
	{
		[DllImport("android", EntryPoint = "ANativeWindow_fromSurface", CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr ANativeWindow_fromSurface(IntPtr jniEnv, IntPtr surface);
	}
}
