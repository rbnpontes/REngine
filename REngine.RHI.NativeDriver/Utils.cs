using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class NativeUtils
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_free(IntPtr ptr);

		public static void Free(IntPtr ptr)
		{
			rengine_free(ptr);
		}
	}
}
