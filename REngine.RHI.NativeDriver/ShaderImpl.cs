using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class ShaderImpl : NativeObject, IShader
	{
		[DllImport(Constants.Lib)]
		static extern byte rengine_shader_gettype(IntPtr shader);

		public ShaderType Type => (ShaderType)rengine_shader_gettype(Handle);

		public string Name
		{
			get => Marshal.PtrToStringAnsi(rengine_object_getname(Handle)) ?? string.Empty;
		}

		public GPUObjectType ObjectType => GPUObjectType.Shader;

		public ShaderImpl(IntPtr handle) : base(handle)
		{
		}

	}
}
