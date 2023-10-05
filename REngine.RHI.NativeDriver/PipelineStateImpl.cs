using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class GraphicsPipelineImpl : BasePipelineStateImpl, IPipelineState
	{
		public GraphicsPipelineDesc Desc { get; private set; }
		public GraphicsPipelineImpl(GraphicsPipelineDesc desc, IntPtr handle) : base(handle) 
		{
			// Remove Shaders 
			desc.Shaders = new GraphicsPipelineShaders();
			Desc = desc;
		}

		protected override string GetName()
		{
			return Desc.Name;
		}
	}
}
