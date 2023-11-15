using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class ShaderResourceBindingImpl : NativeObject, IShaderResourceBinding
	{
		[DllImport(Constants.Lib)]
		static extern void rengine_srb_set(
			IntPtr handle,
			uint flags,
			IntPtr resourceName,
			IntPtr resource
		);

		public ShaderResourceBindingImpl(IntPtr handle) : base(handle)
		{
		}

		public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
		{
			if (TestFlags(flags, ShaderTypeFlags.Vertex))
				SetShaderRes((uint)ShaderTypeFlags.Vertex, resourceName, resource);
			if(TestFlags(flags, ShaderTypeFlags.Pixel))
				SetShaderRes((uint)ShaderTypeFlags.Pixel, resourceName, resource);
			if(TestFlags(flags, ShaderTypeFlags.Compute))
				SetShaderRes((uint)ShaderTypeFlags.Compute, resourceName, resource);
			if(TestFlags(flags, ShaderTypeFlags.Domain))
				SetShaderRes((uint)ShaderTypeFlags.Domain, resourceName, resource);
			if(TestFlags(flags, ShaderTypeFlags.Hull))
				SetShaderRes((uint)ShaderTypeFlags.Hull, resourceName, resource);
			if(TestFlags(flags, ShaderTypeFlags.Geometry))
				SetShaderRes((uint)ShaderTypeFlags.Geometry, resourceName, resource);
		}

		private void SetShaderRes(uint type, string resourceName, IGPUObject resource)
		{
			IntPtr resourceNamePtr = Marshal.StringToHGlobalAnsi(resourceName);
			rengine_srb_set(Handle, type, resourceNamePtr, resource.Handle);
			Marshal.FreeHGlobal(resourceNamePtr);
		}

		private bool TestFlags(ShaderTypeFlags flags, ShaderTypeFlags expected)
		{
			return (flags & expected) != 0;
		}
	}
}
