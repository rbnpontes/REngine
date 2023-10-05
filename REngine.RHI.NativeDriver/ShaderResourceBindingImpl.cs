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
			IntPtr resourceNamePtr = Marshal.StringToHGlobalAnsi(resourceName);

			rengine_srb_set(Handle, (uint)flags, resourceNamePtr, resource.Handle);
			
			Marshal.FreeHGlobal(resourceNamePtr);
		}
	}
}
