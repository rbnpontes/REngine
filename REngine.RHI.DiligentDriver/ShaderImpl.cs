using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class ShaderImpl : GPUObjectImpl, IShader
	{
		public override string Name => GetHandle<Diligent.IShader>().GetDesc().Name;

		public ShaderType Type { get; private set; }

		public ShaderImpl(Diligent.IShader handle) : base(handle)
		{
			var adapter = new ShaderAdapter();

			ShaderType shaderType;
			adapter.Fill(handle.GetDesc().ShaderType, out shaderType);
			Type = shaderType;

			handle.SetUserData(new ObjectWrapper(this));
		}
	}
}
