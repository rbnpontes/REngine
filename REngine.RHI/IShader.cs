using REngine.Core;
using REngine.Core.Mathematics;
using System;
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

		public ulong ToHash()
		{
			ulong hash = Hash.Digest(Name);
			hash = Hash.Combine(hash, (ulong)Type);
			if (string.IsNullOrEmpty(SourceCode))
				hash = Hash.Combine(hash, Hash.Digest(ByteCode));
			else
				hash = Hash.Combine(hash, Hash.Digest(SourceCode));
			foreach (var pair in Macros)
				hash = Hash.Combine(hash, Hash.Combine(Hash.Digest(pair.Key), Hash.Digest(pair.Value)));
			return hash;
		}
	}

	public interface IShader : IGPUObject, IHashable
	{
		public ShaderType Type { get; }
	}
}
