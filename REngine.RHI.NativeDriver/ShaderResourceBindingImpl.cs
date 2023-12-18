using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;

namespace REngine.RHI.NativeDriver
{
	internal class ShaderResourceBindingImpl(IntPtr handle, BasePipelineStateImpl pipelineStateImpl) : NativeObject(handle), IShaderResourceBinding
	{
		[DllImport(Constants.Lib)]
		public static extern void rengine_srb_set(
			IntPtr handle,
			uint flags,
			IntPtr resourceName,
			IntPtr resource
		);

		protected override void BeforeRelease()
		{
			pipelineStateImpl.RemoveShaderResourceBinding(handle);
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

	internal class DefaultShaderResourceBinding(IntPtr handle, BasePipelineStateImpl pipelineState) : ShaderResourceBindingImpl(handle, pipelineState)
	{
		private bool pSkipException;
		protected override void BeforeRelease()
		{
			if(!pSkipException)
				throw new Exception($"Can´t dispose {nameof(IShaderResourceBinding)}. This will be release at pipeline state dispose");
		}

		internal void UnlockRelease()
		{
			pSkipException = true;
		}
	}
}
