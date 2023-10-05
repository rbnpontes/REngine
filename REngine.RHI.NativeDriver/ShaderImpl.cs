using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class ShaderImpl : NativeObject, IShader
	{
		public ShaderType Type => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public ShaderImpl(IntPtr handle) : base(handle)
		{
		}

	}
}
