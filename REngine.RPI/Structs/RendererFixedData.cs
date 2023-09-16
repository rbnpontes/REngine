using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Structs
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RendererFixedData
	{
		public uint ViewWidth;
		public uint ViewHeight;
	}
}
