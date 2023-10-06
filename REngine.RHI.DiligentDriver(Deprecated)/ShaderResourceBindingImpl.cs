using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class ShaderResourceBindingImpl : IDisposable, IShaderResourceBinding, INativeObject
	{
		private Diligent.IShaderResourceBinding? pBinding;
		public object? Handle => pBinding;
		public bool IsDisposed => pBinding == null;
		
		public ShaderResourceBindingImpl(Diligent.IShaderResourceBinding binding)
		{
			pBinding = binding;
		}

		public void Dispose()
		{
			pBinding?.Dispose();
			pBinding = null;
		}

		public void Set(ShaderTypeFlags flags, string resourceName, IGPUObject resource)
		{
			if (pBinding == null)
				throw new ObjectDisposedException("Can´t set resource. IPipelineResourceBinding is already disposed.");
			var adapter = new ShaderAdapter();

			Diligent.ShaderType shaderTypes;
			adapter.Fill(flags, out shaderTypes);

			var vary = pBinding.GetVariableByName(shaderTypes, resourceName);
			if (vary != null)
			{
				var nativeObj = NativeObjectUtils.Get<Diligent.IDeviceObject>(resource);
				vary.Set(nativeObj, Diligent.SetShaderResourceFlags.AllowOverwrite);
			}
		}
	}
}
