using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Structs
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FrameData
	{
		public Matrix4x4 ScreenProjection;
		public Matrix4x4 InvScreenProjection;
		public uint ScreenWidth;
		public uint ScreenHeight;
		public float ElapsedTime;		
		public float DeltaTime;
	}
}
