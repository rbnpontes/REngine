using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class BufferImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_buffer_getdesc(IntPtr buffer, ref BufferDescDTO output);
	}
}
