﻿using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal abstract class BasePipelineStateImpl : NativeObject, IBasePipelineState
	{
		[DllImport(Constants.Lib)]
		protected static extern void rengine_pipelinestate_createresourcebinding(
			IntPtr pipeline,
			ref ResultNative result
		);

		private IShaderResourceBinding? pDefaultSRB;

		public string Name 
		{ 
			get => GetName();
		}

		public BasePipelineStateImpl(IntPtr handle) : base(handle)
		{
		}

		protected abstract string GetName();

		public IShaderResourceBinding CreateResourceBinding()
		{
			ResultNative result = new();
			rengine_pipelinestate_createresourcebinding(Handle, ref result);

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create SRB");
			return new ShaderResourceBindingImpl(result.value);
		}

		public IShaderResourceBinding GetResourceBinding()
		{
			pDefaultSRB ??= CreateResourceBinding();
			return pDefaultSRB;
		}
	}
}