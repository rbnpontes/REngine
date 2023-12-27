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
		[DllImport(Constants.Lib)]

		static extern void rengine_buffer_create_view(IntPtr buffer, ref BufferViewCreateDescDTO createDesc,
			ref ResultNative result);
		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_buffer_get_default_view(IntPtr buffer, byte viewType);

		[DllImport(Constants.Lib)]
		static extern uint rengine_buffer_get_state(IntPtr buffer);
		[DllImport(Constants.Lib)]
		static extern void rengine_buffer_set_state(IntPtr buffer, uint state);

		[DllImport(Constants.Lib)]
		static extern ulong rengine_buffer_get_gpuhandle(IntPtr buffer);
	}
}
