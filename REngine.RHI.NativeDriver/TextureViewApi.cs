using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class TextureViewImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_textureview_getparent(IntPtr texView, ref ResultNative result);
		[DllImport(Constants.Lib)]
		static extern void rengine_textureview_getdesc(IntPtr texView, ref TextureViewDescDTO output);
	}
}
