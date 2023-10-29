using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	[Flags]
	public enum GPUObjectType
	{
		Unknown = 0,
		Texture1D = 1 << 0,
		Texture2D = 1 << 1,
		Texture3D = 1 << 2,
		TextureArray = 1 << 3,
		RenderTarget = 1 << 4,
		TextureView = 1 << 5,
		VertexBuffer = 1 << 6,
		IndexBuffer = 1 << 7,
		ConstantBuffer = 1 << 8,
		UavBuffer = 1 << 9,
		BufferView = 1 << 10,
		GraphicsPipeline = 1 << 11,
		ComputePipeline = 1 << 12,
		Shader = 1 << 13,
		Texture = Texture1D | Texture2D | Texture3D | TextureArray | RenderTarget,
		Buffer = VertexBuffer | IndexBuffer | ConstantBuffer,
		PipelineState = GraphicsPipeline | ComputePipeline,
	}
	public interface IGPUObject : INativeObject
	{
		public GPUObjectType ObjectType { get; }
		public string Name { get; }
	}
}
