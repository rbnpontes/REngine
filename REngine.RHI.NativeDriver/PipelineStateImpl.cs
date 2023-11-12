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
		private readonly ulong pHash;
		public GraphicsPipelineDesc Desc { get; private set; }
		public GraphicsPipelineImpl(GraphicsPipelineDesc desc, IntPtr handle) : base(handle, GPUObjectType.GraphicsPipeline) 
		{
			pHash = desc.ToHash();
			// Remove Shaders 
			desc.Shaders = new GraphicsPipelineShaders();
			Desc = desc;
		}

		protected override string GetName()
		{
			return Desc.Name;
		}

		public override ulong ToHash()
		{
			return pHash;
		}
	}
	
	internal class ComputePipelineImpl : BasePipelineStateImpl, IComputePipelineState
	{
		private readonly ulong pHash;
		public ComputePipelineDesc Desc { get; private set; }
		public ComputePipelineImpl(ComputePipelineDesc desc, IntPtr handle) : base(handle, GPUObjectType.ComputePipeline)
		{
			pHash = desc.ToHash();
			// Remove Compute Shader
			desc.ComputeShader = null;
			Desc = desc;
		}
		protected override string GetName()
		{
			return Desc.Name;
		}

		public override ulong ToHash()
		{
			return pHash;
		}
	}
}
