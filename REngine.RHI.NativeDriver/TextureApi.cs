using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class TextureImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_texture_getdesc(
			IntPtr texture,
			ref TextureDescDTO output
		);
		[DllImport(Constants.Lib)]
		internal static extern void rengine_texture_getdefaultview(
			IntPtr texture,
			byte viewType,
			ref ResultNative result
		);

		[DllImport(Constants.Lib)]
		internal static extern uint rengine_texture_get_state(IntPtr texture);

		[DllImport(Constants.Lib)]
		internal static extern void rengine_texture_set_state(IntPtr texture, uint state);

		[DllImport(Constants.Lib)]
		internal static extern ulong rengine_texture_get_gpuhandle(IntPtr texture);
	}
}
