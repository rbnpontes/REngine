using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class NativeObject
	{
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_releaseref(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_set_release_callback(IntPtr obj, IntPtr releaseCallback);
		[DllImport(Constants.Lib)]
		public static extern IntPtr rengine_object_getname(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_addref(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern uint rengine_object_strongref_count(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern uint rengine_object_weakref_count(IntPtr obj);
	}
}
