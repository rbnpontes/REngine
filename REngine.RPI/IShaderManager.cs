using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public interface IShaderManager
	{
		public IShader GetOrCreate(ShaderCreateInfo createInfo);
		public IShader? FindByHash(ulong hash);
		public IShaderManager ClearCache(bool clearFiles = false);
	}
}
