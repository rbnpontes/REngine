﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct ShaderCreateInfo 
	{
		public string Name;
		public ShaderType Type;
		public string SourceCode;
		public byte[] ByteCode;
		public IDictionary<string, string> Macros;

		public ShaderCreateInfo()
		{
			Name = string.Empty;
			Type = ShaderType.Vertex;
			SourceCode = string.Empty;
			ByteCode = new byte[0];
			Macros = new Dictionary<string, string>();
		}
	}

	public interface IShader : IGPUObject
	{
		public ShaderType Type { get; }
	}
}