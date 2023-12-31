﻿using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{

	public interface IBasePipelineState : IGPUObject, IHashable
	{
		public IShaderResourceBinding[] ShaderResourceBindings { get; }
		public bool HasShaderResourceBinding(IShaderResourceBinding srb);
		public IShaderResourceBinding GetResourceBinding();
		public IShaderResourceBinding CreateResourceBinding();
	}
	public interface IPipelineState : IBasePipelineState
	{
		public GraphicsPipelineDesc Desc { get; }
	}

	public interface IComputePipelineState : IBasePipelineState
	{
		public ComputePipelineDesc Desc { get; }
	}
}
