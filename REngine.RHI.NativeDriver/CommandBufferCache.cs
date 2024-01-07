using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;
using REngine.RHI.NativeDriver.NativeStructs;

namespace REngine.RHI.NativeDriver
{
	internal partial class CommandBufferImpl
	{
		private const byte MaxRenderTargets = 4;
		private const int MaxVertexBuffers = 4;

		private readonly float[] pCopyColor = new float[4] { 0f, 0f, 0f, 0f };
		private DrawAttribsNative pCopyDrawArgs;
		private DrawIndexedAttribsNative pCopyIndexedDrawArgs;
		
		private readonly IntPtr[] pCopyRenderTargetsPointers = new IntPtr[MaxRenderTargets];
		private readonly IntPtr[] pCopyVertexBuffersPointers = new IntPtr[MaxVertexBuffers];
		private readonly ulong[] pCopyOffsets = new ulong[MaxRenderTargets];

		private readonly IntRect[] pCopyScissors = new IntRect[1];
		private readonly Viewport[] pCopyViewport = new Viewport[1];

		private readonly ArrayPool<StateTransitionDTO> pBarriersPool = ArrayPool<StateTransitionDTO>.Create();
		private readonly ArrayPool<IntPtr> pPtrsPool = ArrayPool<IntPtr>.Create();
	}
}
