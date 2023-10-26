using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class DeviceImpl
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_device_create_buffer(
			IntPtr device,
			ref BufferDescDTO desc,
			ulong size,
			IntPtr data,
			ref ResultNative result
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_device_create_shader(
			IntPtr device,
			ref ShaderCreateInfoDTO createInfo,
			ref ResultNative result
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_device_create_graphicspipeline(
			IntPtr device,
			ref GraphicsPipelineDescDTO desc,
			byte isOpenGl,
			ref ResultNative result
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_device_create_computepipeline(
			IntPtr device,
			ref ComputePipelineDescDTO desc,
			byte isOpenGl,
			ref ResultNative result
		);
		[DllImport(Constants.Lib)]
		static extern void rengine_device_create_texture(
			IntPtr device,
			ref TextureDescDTO desc,
			IntPtr data,
			uint numTexData,
			ref ResultNative result
		);
	}
}
