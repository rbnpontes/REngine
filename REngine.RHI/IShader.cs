using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct VertexShaderAttribute
	{
		public VertexElementSemantic Semantic;
		public string Name;
		public string InputIndex;
	}

	public struct ShaderCreateInfo 
	{
		public string Name;
		public ShaderType Type;
		public string SourceCode;
		public byte[] ByteCode;
	}

	public interface IShader : IGPUObject
	{
		public ShaderType Type { get; }
	}
}
