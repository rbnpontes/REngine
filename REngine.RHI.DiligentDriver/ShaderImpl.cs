using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class ShaderImpl : IShader, INativeObject
	{
		private Diligent.IShader? pHandle;
		private ShaderAdapter pAdapter;

		public object? Handle => pHandle;
		public bool IsDisposed => pHandle == null;

		public string Name => pHandle?.GetDesc().Name ?? string.Empty;

		public ShaderType Type { get; private set; }

		public ShaderImpl(Diligent.IShader handle)
		{
			pAdapter = new ShaderAdapter();
			pHandle = handle;

			ShaderType shaderType;
			pAdapter.Fill(handle.GetDesc().ShaderType, out shaderType);
			Type = shaderType;
		}

		public void Dispose()
		{
			pHandle?.Dispose();
			pHandle = null;
		}
	}
}
