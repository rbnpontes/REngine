using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class CommandBufferImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_cleardepth(
			IntPtr context,
			IntPtr depthStencil,
			uint clearFlags,
			float depth,
			byte stencil,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_clearrt(
			IntPtr context,
			IntPtr rt,
			IntPtr color,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_commitbindings(
			IntPtr context,
			IntPtr shaderRes,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_copy_tex(
			IntPtr context,
			ref CopyTextureInfoDTO copyInfo,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_draw(
			IntPtr context,
			ref DrawAttribsNative drawAttribs
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_drawindexed(
			IntPtr context,
			ref DrawIndexedAttribsNative drawAttribs
		);
		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_cmdbuffer_map(
			IntPtr context,
			IntPtr buffer,
			byte mapType,
			byte mapFlags
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_unmap(
			IntPtr context,
			IntPtr buffer,
			byte mapType
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setibuffer(
			IntPtr context,
			IntPtr buffer,
			ulong byteOffset,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setvbuffer(
			IntPtr context,
			uint startSlot,
			uint numBuffers,
			IntPtr buffers,
			IntPtr offsets,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setpipeline(
			IntPtr context,
			IntPtr pipeline
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setrts(
			IntPtr context,
			IntPtr rts,
			byte numRts,
			IntPtr depth,
			byte isDeferred
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_updtbuffer(
			IntPtr context,
			IntPtr buffer,
			ulong offset,
			ulong size,
			IntPtr data,
			byte isDeferred
		);
	}
}
