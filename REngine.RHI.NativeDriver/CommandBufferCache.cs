using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI.NativeDriver.NativeStructs;

namespace REngine.RHI.NativeDriver
{
	internal partial class CommandBufferImpl
	{
		private const byte MAX_RENDER_TARGETS = 4;
		private const int MAX_VERTEX_BUFFERS = 4;

		private readonly float[] pCopyColor = new float[4] { 0f, 0f, 0f, 0f };
		private DrawAttribsNative pCopyDrawArgs;
		private DrawIndexedAttribsNative pCopyIndexedDrawArgs;
		

		private readonly IntPtr[] pCopyRenderTargetsPtrs = new IntPtr[MAX_RENDER_TARGETS];
		private readonly IntPtr[] pCopyVertexBuffersPtrs = new IntPtr[MAX_VERTEX_BUFFERS];
		private ulong[] pCopyOffsets = new ulong[MAX_RENDER_TARGETS];
	}
}
