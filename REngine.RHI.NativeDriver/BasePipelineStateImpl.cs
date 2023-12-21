using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal abstract class BasePipelineStateImpl(IntPtr handle, GPUObjectType objectType)
		: NativeObject(handle), IBasePipelineState
	{
		[DllImport(Constants.Lib)]
		protected static extern void rengine_pipelinestate_createresourcebinding(
			IntPtr pipeline,
			ref ResultNative result
		);
		
		private readonly Dictionary<IntPtr, IShaderResourceBinding> pShaderResourceBindings = new();
		private DefaultShaderResourceBinding? pDefaultShaderResourceBinding;

		public IShaderResourceBinding[] ShaderResourceBindings => pShaderResourceBindings.Values.ToArray();
		public string Name => GetName();
		public GPUObjectType ObjectType => objectType;
		protected abstract string GetName();
		
		private IntPtr AllocateShaderResourceBinding()
		{
			ResultNative result = new();
			rengine_pipelinestate_createresourcebinding(Handle, ref result);
			
			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? $"Could not possible to create {nameof(IShaderResourceBinding)}.");
			if (result.value == IntPtr.Zero)
				throw new NullReferenceException($"Could not possible to create {nameof(IShaderResourceBinding)}.");
			return result.value;
		}
		public IShaderResourceBinding CreateResourceBinding()
		{
			return new ShaderResourceBindingImpl(AllocateShaderResourceBinding(), this);
		}
		public bool HasShaderResourceBinding(IShaderResourceBinding srb)
		{
			return pShaderResourceBindings.ContainsKey(srb.Handle);
		}
		public IShaderResourceBinding GetResourceBinding()
		{
			pDefaultShaderResourceBinding ??= new DefaultShaderResourceBinding(AllocateShaderResourceBinding(), this);
			return pDefaultShaderResourceBinding;
		}
		protected override void BeforeRelease()
		{
			if (pDefaultShaderResourceBinding is null)
				return;
			if (pDefaultShaderResourceBinding.IsDisposed)
				return;
			pDefaultShaderResourceBinding.UnlockRelease();
			pDefaultShaderResourceBinding.Dispose();
			
			foreach(var pair in pShaderResourceBindings)
				pair.Value.Dispose();
			pShaderResourceBindings.Clear();
		}
		public abstract ulong ToHash();
		public void RemoveShaderResourceBinding(IntPtr ptr)
		{
			pShaderResourceBindings.Remove(ptr);
		}
	}
}
