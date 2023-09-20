using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	internal class SpriteBatchFeature : IRenderFeature
	{
		struct BufferData
		{
			public Vector4 RotationAndScale;
			public Vector4 Position;
			public Vector4 Color;
		}
		class TextureCache : IDisposable
		{
			public ITexture Texture;
			public IShaderResourceBinding ResourceBinding;

			public TextureCache(ITexture texture, IShaderResourceBinding resourceBinding)
			{
				Texture = texture;
				ResourceBinding = resourceBinding;
			}

			public void Dispose()
			{
				Texture.Dispose();
				ResourceBinding.Dispose();
			}
		}
		
		private GraphicsSettings pSettings;
		private RenderSettings pRenderSettings;

		private IPipelineState? pDefaultPipeline;
		private IPipelineState? pTexturedPipeline;
		private IBuffer? pCBuffer;
		private IRenderer? pRenderer;
		private IGraphicsDriver? pDriver;

		private SpriteBatcher pBatcher;
		private SpriteTextureManager pTextureManager;

		public bool IsDirty { get; private set; } = true;
		public bool IsDisposed { get; private set; } = false;

		public SpriteBatchFeature(
			SpriteBatcher batcher, 
			SpriteTextureManager texManager, 
			GraphicsSettings settings, 
			RenderSettings renderSettings,
			RendererEvents renderEvents
		)
		{
			pBatcher = batcher;
			pTextureManager = texManager;
			pSettings = settings;
			pRenderSettings = renderSettings;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;

			IsDisposed = true;
			pDefaultPipeline?.Dispose();
			pTexturedPipeline?.Dispose();

			pDefaultPipeline = pTexturedPipeline = null;
			pTexturedPipeline = null;
		}

		public IRenderFeature Setup(IGraphicsDriver driver, IRenderer renderer)
		{
			pRenderer = renderer;
			pDriver = driver;
			pCBuffer = renderer.GetBuffer(BufferGroupType.Object);

			IShader vertexShader = LoadShader(driver.Device, ShaderType.Vertex, false);
			IShader pixelShader = LoadShader(driver.Device, ShaderType.Pixel, false);
			IShader texturePixelShader = LoadShader(driver.Device, ShaderType.Pixel, true);

			IPipelineState defaultPipeline = CreatePipeline(driver.Device, renderer, vertexShader, pixelShader);
			IPipelineState texturedPipeline = CreatePipeline(driver.Device, renderer, vertexShader, texturePixelShader);

			pDefaultPipeline = defaultPipeline;
			pTexturedPipeline = texturedPipeline;

			vertexShader.Dispose();
			pixelShader.Dispose();
			texturePixelShader.Dispose();

			IsDirty = false;
			return this;
		}

		public IRenderFeature Compile(ICommandBuffer command)
		{
			return this;
		}

		public IRenderFeature Execute(ICommandBuffer command)
		{
			var items = pBatcher.Items;
			BufferData cbufferData = new BufferData();

			if (pRenderer?.SwapChain is null || pDriver is null)
				return this;

			if (pDefaultPipeline is null || pTexturedPipeline is null)
				return this;

			command.SetRTs(new ITextureView[] { pRenderer.SwapChain.ColorBuffer }, pRenderer.SwapChain.DepthBuffer);
			for(int i =0; i < pBatcher.BatchCount; ++i)
			{
				var item = items[i];
				var pipeline = item.TextureIndex == byte.MaxValue ? pDefaultPipeline : pTexturedPipeline;
				FillBufferData(item, ref cbufferData);

				var mappedData = command.Map<BufferData>(pCBuffer, MapType.Write, MapFlags.Discard);
				mappedData[0] = cbufferData;
				command.Unmap(pCBuffer, MapType.Write);

				if (item.TextureIndex != byte.MaxValue && pTextureManager.Textures[item.TextureIndex] != null)
					pipeline.GetResourceBinding().Set(ShaderTypeFlags.Pixel, "g_texture", pTextureManager.Textures[item.TextureIndex]);

				command
					.SetPipeline(pipeline)
					.CommitBindings(pipeline.GetResourceBinding())
					.Draw(new DrawArgs
					{
						NumVertices = 4
					});
			}
				
			return this;
		}

		public IRenderFeature MarkAsDirty()
		{
			IsDirty = true;
			return this;
		}

		private void FillBufferData(SpriteBatchItem item, ref BufferData data)
		{
			float sinAngle = (float)Math.Sin(item.Angle);
			float cosAngle = (float)Math.Cos(item.Angle);

			var scaleMatrix = Matrix<float>.Build.Dense(2, 2);
			var rotMatrix = Matrix<float>.Build.Dense(2, 2);
			{
				scaleMatrix[0, 0] = item.Size.X;
				scaleMatrix[1, 1] = item.Size.Y;

				rotMatrix[0, 0] = cosAngle;
				rotMatrix[0, 1] = -sinAngle;
				rotMatrix[1, 0] = sinAngle;
				rotMatrix[1, 1] = cosAngle;
			}

			var matrix = scaleMatrix * rotMatrix;

			data.RotationAndScale = new Vector4(matrix[0, 0], matrix[1, 0], matrix[0, 1], matrix[1, 1]);
			data.Position = new Vector4(item.Position.X, item.Position.Y, item.Offset.X, item.Offset.Y);
			data.Color = Vector4.One;
		}

		private IPipelineState CreatePipeline(IDevice device, IRenderer renderer, IShader vshader, IShader pshader) 
		{
			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Spritebatch PSO";

			desc.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			desc.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;
			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleStrip;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = true;

			desc.Shaders.VertexShader = vshader;
			desc.Shaders.PixelShader = pshader;

			desc.Samplers.Add(new ImmutableSamplerDesc
			{
				Name = "g_texture",
				Sampler = new SamplerStateDesc(TextureFilterMode.Trilinear, TextureAddressMode.Clamp)
			});

			var pipeline = device.CreateGraphicsPipeline(desc);
			pipeline.GetResourceBinding().Set(ShaderTypeFlags.Vertex, "Constants", renderer.GetBuffer(BufferGroupType.Object));
			return pipeline;
		}

		private IShader LoadShader(IDevice device, ShaderType type, bool hasTexture)
		{
			string shaderPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Shaders");
			ShaderCreateInfo shaderCI = new ShaderCreateInfo
			{
				Type = type
			};

			switch (type)
			{
				case ShaderType.Vertex:
					{
						shaderCI.Name = "Spritebatch Vertex Shader";
						shaderPath = Path.Join(shaderPath, "spritebatch_vs.hlsl");
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Spritebatch Pixel Shader";
						shaderPath = Path.Join(shaderPath, "spritebatch_ps.hlsl");
					}
					break;
				default:
					throw new NotImplementedException();
			}

			shaderCI.SourceCode = File.ReadAllText(shaderPath);
			if (hasTexture)
			{
				shaderCI.Name = shaderCI.Name + "(TEXTURED)";
				shaderCI.Macros.Add("RENGINE_ENABLED_TEXTURE", "1");
			}

			return device.CreateShader(shaderCI);
		}
	}
}
