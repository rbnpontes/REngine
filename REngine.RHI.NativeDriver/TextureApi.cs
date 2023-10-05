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
		static extern void rengine_texture_getdefaultview(
			IntPtr texture,
			byte viewType,
			ref ResultNative result
		);
	}
}
