using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	internal class SpriteBatchFeature : IRenderFeature
	{
		[Flags]
		public enum DirtyFlags
		{
			None =0,
			Pipeline=1,
			BindingInvalid = 2,
			Bindings = 4,
			CBuffer = 8,
			All = Pipeline | BindingInvalid | Bindings | CBuffer
		}

		struct BufferData
		{
			public Matrix4x4 Transform;
			public Matrix4x4 Projection;
			public Vector4 Color;

			public BufferData()
			{
				Transform = Projection = Matrix4x4.Identity;
				Color = Vector4.One;
			}
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

		private static readonly Vector4[] sVertices = new Vector4[] 
		{ 
			new Vector4(0, 1, 0, 1),
			new Vector4(1, 0, 1, 0),
			new Vector4(0, 0, 0, 0),

			new Vector4(0, 1, 0, 1),
			new Vector4(1, 1, 1, 1),
			new Vector4(1, 0, 1, 0)
		};

		private GraphicsSettings pSettings;

		private IShaderResourceBinding?[] pBindings;

		private IPipelineState? pDefaultPipeline;
		private IPipelineState? pTexturedPipeline;
		private IBuffer? pCBuffer;
		private IRenderer? pRenderer;
		private IGraphicsDriver? pDriver;

		private IBuffer? pVBuffer;
		private SpriteBatcher pBatcher;
		private SpriteTextureManager pTextureManager;

		private DirtyFlags pDirtyFlags = DirtyFlags.All;
		public bool IsDirty { get => pDirtyFlags != DirtyFlags.None; }
		public bool IsDisposed { get; private set; } = false;

		public SpriteBatchFeature(
			SpriteBatcher batcher, 
			SpriteTextureManager texManager, 
			GraphicsSettings settings
		)
		{
			pBatcher = batcher;
			pTextureManager = texManager;
			pSettings = settings;
			texManager.OnUpdateTextures += HandleTextureUpdate;
			pBindings = new IShaderResourceBinding[texManager.Textures.Length];
		}

		private void HandleTextureUpdate(object? sender, EventArgs e)
		{
			DirtyFlags dirtyFlags = DirtyFlags.Bindings;
			if(pTextureManager.Textures.Length != pBindings.Length)
			{
				pBindings = new IShaderResourceBinding[pTextureManager.Textures.Length];
				dirtyFlags |= DirtyFlags.BindingInvalid;
			}

			MarkAsDirty(dirtyFlags);
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;

			IsDisposed = true;
			for(int i = 0; i < pBindings.Length; ++i)
			{
				pBindings[i]?.Dispose();
				pBindings[i] = null;
			}
			pDefaultPipeline?.Dispose();
			pTexturedPipeline?.Dispose();
			pVBuffer?.Dispose();


			pDefaultPipeline = pTexturedPipeline = null;
			pTexturedPipeline = null;
		}

		public void CheckCBufferSizes(ulong cbufferSize)
		{
			if (cbufferSize != pCBuffer?.Desc.Size)
				MarkAsDirty(DirtyFlags.CBuffer);
		}

		public IRenderFeature Setup(IGraphicsDriver driver, IRenderer renderer)
		{
			pRenderer = renderer;
			pDriver = driver;
			pCBuffer = renderer.GetBuffer(BufferGroupType.Object);

			if((pDirtyFlags & DirtyFlags.Pipeline) != 0)
			{
				pDefaultPipeline?.Dispose();
				pTexturedPipeline?.Dispose();
				pVBuffer?.Dispose();

				IShader vertexShader = LoadShader(driver.Device, ShaderType.Vertex, false);
				IShader pixelShader = LoadShader(driver.Device, ShaderType.Pixel, false);
				IShader texturePixelShader = LoadShader(driver.Device, ShaderType.Pixel, true);

				IPipelineState defaultPipeline = CreatePipeline(driver.Device, renderer, vertexShader, pixelShader);
				IPipelineState texturedPipeline = CreatePipeline(driver.Device, renderer, vertexShader, texturePixelShader);

				pVBuffer = CreateVertexBuffer(driver.Device);
				pDefaultPipeline = defaultPipeline;
				pTexturedPipeline = texturedPipeline;

				vertexShader.Dispose();
				pixelShader.Dispose();
				texturePixelShader.Dispose();
			}

			if((pDirtyFlags & DirtyFlags.BindingInvalid) != 0)
			{
				for(byte i =0; i < pBindings.Length; ++i)
				{
					var binding = pBindings[i];

					binding?.Dispose();
					binding = pTexturedPipeline?.CreateResourceBinding();
					pBindings[i] = binding;

					SetBinding(i);
				}
				// remove bindings flag because we already this step while is creating binding
				pDirtyFlags ^= DirtyFlags.Bindings;
			}

			if((pDirtyFlags & DirtyFlags.Bindings) != 0)
			{
				for(byte i =0; i< pBindings.Length; ++i)
					SetBinding(i);
			}

			if((pDirtyFlags & DirtyFlags.CBuffer) != 0)
			{
				SetCBufferBinding(pDefaultPipeline?.GetResourceBinding());
				for (byte i = 0; i < pBindings.Length; ++i)
					SetCBufferBinding(pBindings[i]);
			}

			pDirtyFlags = DirtyFlags.None;
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

			if (pDefaultPipeline is null || pTexturedPipeline is null || pVBuffer is null)
				return this;

			Matrix4x4 projection;
			CalculateProjection(pRenderer.SwapChain.Size, out projection);
			cbufferData.Projection = projection;

			command.SetRTs(new ITextureView[] { pRenderer.SwapChain.ColorBuffer }, pRenderer.SwapChain.DepthBuffer);
			for(int i =0; i < pBatcher.BatchCount; ++i)
			{
				var item = items[i];
				byte textureSlot = item.TextureSlot;
				var pipeline = textureSlot == byte.MaxValue ? pDefaultPipeline : pTexturedPipeline;
				FillBufferData(item, ref cbufferData);

				var mappedData = command.Map<BufferData>(pCBuffer, MapType.Write, MapFlags.Discard);
				mappedData[0] = cbufferData;
				command.Unmap(pCBuffer, MapType.Write);

				command
					.SetVertexBuffer(pVBuffer)
					.SetPipeline(pipeline);

				if (textureSlot != byte.MaxValue && pBindings[i] != null)
					command.CommitBindings(pBindings[textureSlot]);
				else
					command.CommitBindings(pDefaultPipeline.GetResourceBinding());

				command.Draw(new DrawArgs { NumVertices = 6 });
			}
				
			return this;
		}

		public IRenderFeature MarkAsDirty()
		{
			pDirtyFlags = DirtyFlags.All;
			return this;
		}
		public IRenderFeature MarkAsDirty(DirtyFlags flags)
		{
			pDirtyFlags |= flags;
			return this;
		}

		private void FillBufferData(SpriteBatchInfo item, ref BufferData data)
		{
			var model = Matrix4x4.CreateScale(new Vector3(item.Size, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3((item.Size * item.Anchor) * new Vector2(-1), 0));
			model = model * Matrix4x4.CreateRotationZ(item.Angle) * Matrix4x4.CreateTranslation(new Vector3(item.Position, 0.0f));
			data.Transform = model;

			data.Color = new Vector4(
				item.Color.R / 255.0f,
				item.Color.G / 255.0f,
				item.Color.B / 255.0f,
				item.Color.A / 255.0f
			);
		}

		private void CalculateProjection(in SwapChainSize size, out Matrix4x4 matrix)
		{
			matrix = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0.0f, 1.0f);
			matrix.M33 = matrix.M43 = 0.5f;
		}

		private IPipelineState CreatePipeline(IDevice device, IRenderer renderer, IShader vshader, IShader pshader) 
		{
			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Spritebatch PSO";

			desc.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			desc.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;
			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = false;

			desc.Shaders.VertexShader = vshader;
			desc.Shaders.PixelShader = pshader;

			desc.InputLayouts.Add(
				new PipelineInputLayoutElementDesc
				{
					InputIndex =0,
					Input = new InputLayoutElementDesc {
						ElementType = ElementType.Vector2
					}
				}
			);
			desc.InputLayouts.Add(
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 1,
					Input = new InputLayoutElementDesc
					{
						ElementType = ElementType.Vector2
					}
				}
			);

			desc.Samplers.Add(new ImmutableSamplerDesc
			{
				Name = "g_texture",
				Sampler = new SamplerStateDesc(TextureFilterMode.Trilinear, TextureAddressMode.Clamp)
			});

			return device.CreateGraphicsPipeline(desc);
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
	
		private IBuffer CreateVertexBuffer(IDevice device)
		{
			return device.CreateBuffer(new BufferDesc
			{
				Name = "Spritebatch VBuffer",
				Usage = Usage.Immutable,
				BindFlags = BindFlags.VertexBuffer,
			}, sVertices);
		}

		private void SetBinding(byte slot)
		{
			ITextureView? tex = pTextureManager.Textures[slot]?.GetDefaultView(TextureViewType.ShaderResource);
			if(tex != null)
				pBindings[slot]?.Set(ShaderTypeFlags.Pixel, "g_texture", tex);
		}
		private void SetCBufferBinding(IShaderResourceBinding? binding)
		{
			if (binding is null || pCBuffer is null)
				return;

			binding.Set(ShaderTypeFlags.Vertex, "Constants", pCBuffer);
		}
	}
}
