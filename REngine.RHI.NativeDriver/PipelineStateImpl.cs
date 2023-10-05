using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class GraphicsPipelineImpl : NativeObject, IPipelineState
	{
		public GraphicsPipelineDesc Desc { get; private set; }

		public string Name => throw new NotImplementedException();

		public GraphicsPipelineImpl(GraphicsPipelineDesc desc, IntPtr handle) : base(handle) 
		{
			// Remove Shaders 
			desc.Shaders = new GraphicsPipelineShaders();
			Desc = desc;
		}

		public IShaderResourceBinding CreateResourceBinding()
		{
			throw new NotImplementedException();
		}

		public IShaderResourceBinding GetResourceBinding()
		{
			throw new NotImplementedException();
		}
	}
}
