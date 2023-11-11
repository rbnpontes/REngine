using REngine.Core.Serialization;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Serialization
{
	internal class ShaderRefs
	{
		public ulong VS;
		public ulong PS;
		public ulong DS;
		public ulong HS;
		public ulong GS;
		public ulong CS;
	}
	internal class PipelineSerializationData
	{
		public ShaderRefs ShaderRefs { get; set; } = new();
		public GraphicsPipelineDesc? Graphics { get; set; }
		public ComputePipelineDesc? Compute { get; set; }
		public PipelineType Type { get; set; } = PipelineType.Unknow;
	}
	public sealed class PipelineSerializer : IDisposable
	{

		private readonly Stream pStream;
		private readonly List<object> pData = new();
		private bool pDisposed = false;
		public PipelineSerializer(Stream stream) 
		{
			pStream = stream;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			using (TextWriter writer = new StreamWriter(pStream))
				writer.Write(pData.ToJson());

			pDisposed = true;
		}

		public PipelineSerializer AddDesc(GraphicsPipelineDesc desc)
		{
			pData.Add(new PipelineSerializationData
			{
				Type = PipelineType.Graphics,
				Graphics = desc,
				ShaderRefs = new ShaderRefs
				{
					VS = desc.Shaders.VertexShader?.ToHash() ?? 0,
					PS = desc.Shaders.PixelShader?.ToHash() ?? 0,
					DS = desc.Shaders.DomainShader?.ToHash() ?? 0,
					HS = desc.Shaders.HullShader?.ToHash() ?? 0,
					GS = desc.Shaders.GeometryShader?.ToHash() ?? 0,
				}
			});
			return this;
		}
		public PipelineSerializer AddDesc(ComputePipelineDesc desc)
		{
			pData.Add(new PipelineSerializationData
			{
				Type = PipelineType.Compute,
				Compute = desc,
				ShaderRefs = new ShaderRefs { CS = desc.ComputeShader?.ToHash() ?? 0}
			});
			return this;
		}
	}
	public sealed class PipelineDeserializer : IDisposable
	{
		private readonly Stream pStream;

		private bool pDisposed;
		private GraphicsPipelineDesc[] pGraphicsPipelineDesc = Array.Empty<GraphicsPipelineDesc>();
		private ComputePipelineDesc[] pComputeDesc = Array.Empty<ComputePipelineDesc>();
		public PipelineDeserializer(Stream stream)
		{
			pStream = stream;
		}
		public void Dispose()
		{
			if(pDisposed) 
				return;
			pDisposed = true;
		}

		public PipelineDeserializer Deserialize(IShaderManager shaderManager)
		{
			PipelineSerializationData[] serializerData;
			using (TextReader reader = new StreamReader(pStream))
			{
				serializerData = reader.ReadToEnd().FromJson<PipelineSerializationData[]>() ??
				                 Array.Empty<PipelineSerializationData>();
			}

			int maxComputeCount;
			var maxGraphicsCount = maxComputeCount = 0;

			foreach (var data in serializerData)
			{
				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				switch (data.Type)
				{
					case PipelineType.Graphics:
						++maxGraphicsCount;
						break;
					case PipelineType.Compute:
						++maxComputeCount;
						break;
					default:
						throw new NotSupportedException($"Not supported this pipeline type '{data.Type}'");
				}
			}

			pGraphicsPipelineDesc = new GraphicsPipelineDesc[maxGraphicsCount];
			pComputeDesc = new ComputePipelineDesc[maxComputeCount];

			maxGraphicsCount = maxComputeCount = 0;

			foreach (var data in serializerData)
			{
				switch (data.Type)
				{
					case PipelineType.Graphics:
					{
						var graphicsDesc = data.Graphics ?? throw new NullReferenceException(
							"Error occurred at Deserialize Graphics Pipeline. Graphics Description is null");
						graphicsDesc.Shaders.VertexShader = shaderManager.FindByHash(data.ShaderRefs.VS);
						graphicsDesc.Shaders.PixelShader = shaderManager.FindByHash(data.ShaderRefs.PS);
						graphicsDesc.Shaders.DomainShader = shaderManager.FindByHash(data.ShaderRefs.DS);
						graphicsDesc.Shaders.HullShader = shaderManager.FindByHash(data.ShaderRefs.HS);
						graphicsDesc.Shaders.GeometryShader = shaderManager.FindByHash(data.ShaderRefs.GS);
						pGraphicsPipelineDesc[maxGraphicsCount++] = graphicsDesc;
					}
						break;
					case PipelineType.Compute:
					{
						var computeDesc = data.Compute ?? throw new NullReferenceException(
							"Error occurred at Deserialize Compute Pipeline. Compute Description is null");
						computeDesc.ComputeShader = shaderManager.FindByHash(data.ShaderRefs.CS);
						pComputeDesc[maxComputeCount++] = computeDesc;
					}
						break;
					case PipelineType.Mesh:
						break;
					case PipelineType.RayTracing:
						break;
					case PipelineType.Tile:
						break;
					case PipelineType.Unknow:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return this;
		}

		public GraphicsPipelineDesc[] GetGraphicsDescriptions()
		{
			return pGraphicsPipelineDesc;
		}

		public ComputePipelineDesc[] GetComputeDescriptions()
		{
			return pComputeDesc;
		}
	}
}
