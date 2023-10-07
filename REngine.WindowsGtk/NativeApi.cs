using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal static class NativeApi
	{

		public static void GetNativeWindow(Gtk.Window window, out NativeWindow output)
		{
			output = new NativeWindow();
		}
	}
}
