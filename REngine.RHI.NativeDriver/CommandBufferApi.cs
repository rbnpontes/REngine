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
			byte reset,
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
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setblendfactors(
			IntPtr context,
			float r,
			float g,
			float b,
			float a
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setviewports(
			IntPtr context,
			IntPtr viewports,
			byte numViewports,
			uint rtWidth,
			uint rtHeight
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_setscissors(
			IntPtr context,
			IntPtr scissors,
			byte numScissors,
			uint rtWidth,
			uint rtHeight
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_compute(
			IntPtr context,
			ref ComputeArgs args
		);
#if DEBUG
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_begin_dbg_grp(
			IntPtr context,
			IntPtr name,
			IntPtr color
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_end_dbg_grp(
			IntPtr context
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_insert_dbg_label(
			IntPtr context,
			IntPtr label,
			IntPtr color
		);
#endif
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_begin(IntPtr context, uint immediateCtxId);

		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_finish_frame(IntPtr context);

		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_cmdbuffer_finish_command_list(IntPtr context);

		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_transition_shader_resources(
			IntPtr context,
			IntPtr pipelineState,
			IntPtr shaderResourceBinding);

		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_set_stencil_ref(IntPtr context, uint stencilRef);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_invalidate_state(IntPtr context);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_next_subpass(IntPtr context);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_generate_mips(IntPtr context, IntPtr texture);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_transition_resource_states(
			IntPtr context,
			uint barrierCount,
			IntPtr resourceBarriers);
		[DllImport(Constants.Lib)]
		static extern void rengine_cmdbuffer_resolve_texture_subresource(
			IntPtr context,
			IntPtr srcTexture,
			IntPtr dstTexture,
			ref ResolveTextureSubresourceDTO desc);
	}
}
