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
		struct InstancedBufferData
		{
			public Matrix4x4 Projection;
			public Vector4 Color;

			public InstancedBufferData()
			{
				Projection = Matrix4x4.Identity;
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
		private IShaderResourceBinding?[] pInstancedBindings;

		private IPipelineState? pDefaultPipeline;
		private IPipelineState? pTexturedPipeline;
		private IPipelineState? pInstancedPipeline;
		private IPipelineState? pTexturedInstancedPipeline;

		private IBuffer? pCBuffer;
		private IRenderer? pRenderer;
		private IGraphicsDriver? pDriver;

		private IBuffer? pVBuffer;
		private IBuffer? pInstanceBuffer;
		private SpriteBatcher pBatcher;
		private SpriteTextureManager pTextureManager;
		private IBufferProvider? pBufferProvider;

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
			pInstancedBindings = new IShaderResourceBinding[texManager.Textures.Length];
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

		public IRenderFeature Setup(RenderFeatureSetupInfo setupInfo)
		{
			pRenderer = setupInfo.Renderer;
			pDriver = setupInfo.Driver;
			pCBuffer = setupInfo.BufferProvider.GetBuffer(BufferGroupType.Object);
			pBufferProvider = setupInfo.BufferProvider;

			IDevice device = setupInfo.Driver.Device;

			if((pDirtyFlags & DirtyFlags.Pipeline) != 0)
			{
				pDefaultPipeline?.Dispose();
				pTexturedPipeline?.Dispose();
				pVBuffer?.Dispose();

				IShader vertexShader = LoadShader(device, ShaderType.Vertex, false, false);
				IShader instancedVertexShader = LoadShader(device, ShaderType.Vertex, false, true);
				IShader pixelShader = LoadShader(device, ShaderType.Pixel, false, false);
				IShader texturedPixelShader = LoadShader(device, ShaderType.Pixel, true, false);

				IPipelineState defaultPipeline = CreatePipeline(device, vertexShader, pixelShader, false);
				IPipelineState texturedPipeline = CreatePipeline(device, vertexShader, texturedPixelShader, false);
				IPipelineState instancedPipeline = CreatePipeline(device, instancedVertexShader, pixelShader, true);
				IPipelineState instancedTexturedPipeline = CreatePipeline(device, instancedVertexShader, texturedPixelShader, true);

				pVBuffer = CreateVertexBuffer(device);
				pDefaultPipeline = defaultPipeline;
				pTexturedPipeline = texturedPipeline;

				pInstancedPipeline = instancedPipeline;
				pTexturedInstancedPipeline = instancedTexturedPipeline;

				vertexShader.Dispose();
				pixelShader.Dispose();
				texturedPixelShader.Dispose();
			}

			IShaderResourceBinding?[][] bindingArrays = new IShaderResourceBinding?[][]
			{
				pBindings, pInstancedBindings
			};

			if((pDirtyFlags & DirtyFlags.BindingInvalid) != 0)
			{
				for (var j =0; j < bindingArrays.Length; ++j)
				{
					var bindings = bindingArrays[j];
					for (byte i = 0; i < bindings.Length; ++i)
					{
						var binding = bindings[i];

						binding?.Dispose();

						if (j == 0)
							binding = pTexturedPipeline?.CreateResourceBinding();
						else if(j == 1)
							binding = pTexturedInstancedPipeline?.CreateResourceBinding();

						pBindings[i] = binding;

						SetBinding(binding, i);
					}
				}
				// remove bindings flag because we already this step while is creating binding
				pDirtyFlags ^= DirtyFlags.Bindings;
			}

			if((pDirtyFlags & DirtyFlags.Bindings) != 0)
			{
				foreach(var bindings in bindingArrays)
				{
					for(byte i =0; i< bindings.Length; ++i)
						SetBinding(bindings[i], i);
				}
			}

			if((pDirtyFlags & DirtyFlags.CBuffer) != 0)
			{
				SetCBufferBinding(pDefaultPipeline?.GetResourceBinding());
				SetCBufferBinding(pInstancedPipeline?.GetResourceBinding());

				foreach(var bindings in bindingArrays)
				{
					for (byte i = 0; i < bindings.Length; ++i)
						SetCBufferBinding(bindings[i]);
				}
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
			BufferData cbufferData = new BufferData();
			InstancedBufferData instancedBufferData = new InstancedBufferData();

			if (pRenderer?.SwapChain is null || pDriver is null)
				return this;

			if (pDefaultPipeline is null || pTexturedPipeline is null || pVBuffer is null)
				return this;

			Matrix4x4 projection;
			CalculateProjection(pRenderer.SwapChain.Size, out projection);

			cbufferData.Projection = projection;
			instancedBufferData.Projection = projection;

			command.SetRTs(new ITextureView[] { pRenderer.SwapChain.ColorBuffer }, pRenderer.SwapChain.DepthBuffer);
			
			ExecuteIndexed(command, pVBuffer, pDefaultPipeline, pTexturedPipeline, ref cbufferData);
			ExecuteInstanced(command, pVBuffer, pInstancedPipeline, pTexturedInstancedPipeline, ref instancedBufferData);
			return this;
		}

		private void ExecuteIndexed(
			ICommandBuffer cmd, 
			IBuffer vertexBuffer,
			IPipelineState defaultPipeline, 
			IPipelineState texturedPipeline,
			ref BufferData cbufferData)
		{
			var items = pBatcher.Items;
			for (int i =0; i < items.Count; ++i)
			{
				var item = items[i];
				byte textureSlot = item.TextureSlot;
				var pipeline = textureSlot == byte.MaxValue ? defaultPipeline : texturedPipeline;
				FillBufferData(item, ref cbufferData);

				var mappedData = cmd.Map<BufferData>(pCBuffer, MapType.Write, MapFlags.Discard);
				mappedData[0] = cbufferData;
				cmd.Unmap(pCBuffer, MapType.Write);

				cmd
					.SetVertexBuffer(vertexBuffer)
					.SetPipeline(pipeline);

				if (textureSlot != byte.MaxValue && pBindings[i] != null)
					cmd.CommitBindings(pBindings[textureSlot]);
				else
					cmd.CommitBindings(defaultPipeline.GetResourceBinding());

				cmd.Draw(new DrawArgs { NumVertices = 6 });
			}
		}

		private void ExecuteInstanced(
			ICommandBuffer cmd,
			IBuffer vertexBuffer,
			IPipelineState defaultPipeline,
			IPipelineState texturePipeline,
			ref InstancedBufferData bufferData)
		{
			if (pBufferProvider is null)
				return;

			var batches = pBatcher.InstancedItems;

			if (batches.Count == 0)
				return;

			var mappedData = cmd.Map<InstancedBufferData>(pCBuffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = bufferData;
			cmd.Unmap(pCBuffer, MapType.Write);

			ulong requiredInstanceBufferSize = 0;
			int transformSize = Marshal.SizeOf<Matrix4x4>();
			// before render, we must know how big will be our data
			// to transfer instance data into GPU
			foreach(var batch in batches)
				requiredInstanceBufferSize = Math.Max(requiredInstanceBufferSize, (ulong)(batch.Item2.Count() * transformSize));

			// we also store instance buffer
			// this prevents to reexecute buffer provider to get a new one at every frame
			// So we only change get new instance buffer if required data is greater than buffer.
			if(pInstanceBuffer?.Size < requiredInstanceBufferSize || pInstanceBuffer is null)
				pInstanceBuffer = pBufferProvider.GetInstancingBuffer(requiredInstanceBufferSize);


			IBuffer[] vbuffers = new IBuffer[] { vertexBuffer, pInstanceBuffer };

			for(int i =0; i < batches.Count; ++i)
			{
				(byte textureSlot, IEnumerable<SpriteInstancedBatchInfo> items) = batches[i];

				ReadOnlySpan<Matrix4x4> transforms = new ReadOnlySpan<Matrix4x4>(items.Select(x =>
				{
					// apply transform
					return Matrix4x4.Transpose(GetTransform(x.Position, x.Anchor, x.Angle, x.Size));
				}).ToArray());

				IShaderResourceBinding? binding = textureSlot != byte.MaxValue ? pInstancedBindings[textureSlot] : defaultPipeline.GetResourceBinding();
				if (binding is null)
					binding = defaultPipeline.GetResourceBinding();

				cmd
					.UpdateBuffer(pInstanceBuffer, 0, transforms)
					.SetVertexBuffers(0, vbuffers)
					.SetPipeline(textureSlot != byte.MaxValue ? texturePipeline : defaultPipeline)
					.CommitBindings(binding)
					.Draw(new DrawArgs
					{
						NumVertices = 6,
						NumInstances = (uint)transforms.Length
					});
			}
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
			data.Transform = GetTransform(item.Position, item.Anchor, item.Angle, item.Size);
			data.Color = new Vector4(
				item.Color.R / 255.0f,
				item.Color.G / 255.0f,
				item.Color.B / 255.0f,
				item.Color.A / 255.0f
			);
		}
		private Matrix4x4 GetTransform(in Vector2 position, in Vector2 anchor, float rotation, in Vector2 scale)
		{
			Matrix4x4 transform = Matrix4x4.CreateScale(new Vector3(scale, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3((scale * anchor) * new Vector2(-1), 0));
			return transform * Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));
		}

		private void CalculateProjection(in SwapChainSize size, out Matrix4x4 matrix)
		{
			matrix = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0.0f, 1.0f);
			matrix.M33 = matrix.M43 = 0.5f;
		}

		private IPipelineState CreatePipeline(IDevice device, IShader vshader, IShader pshader, bool instanced) 
		{
			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Spritebatch PSO";
			if (instanced)
				desc.Name = desc.Name + "(Instanced)";

			desc.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			desc.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;
			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = true;

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
			if(instanced)
			{
				desc.InputLayouts.Add(
					new PipelineInputLayoutElementDesc { 
						InputIndex = 2,
						Input = new InputLayoutElementDesc { 
							BufferIndex = 1,
							ElementType = ElementType.Vector4,
							InstanceStepRate = 1
						}
					}
				);
				desc.InputLayouts.Add(
					new PipelineInputLayoutElementDesc
					{
						InputIndex = 3,
						Input = new InputLayoutElementDesc
						{
							BufferIndex = 1,
							ElementType = ElementType.Vector4,
							InstanceStepRate = 1
						}
					}
				);
				desc.InputLayouts.Add(
					new PipelineInputLayoutElementDesc
					{
						InputIndex = 4,
						Input = new InputLayoutElementDesc
						{
							BufferIndex = 1,
							ElementType = ElementType.Vector4,
							InstanceStepRate = 1
						}
					}
				);
				desc.InputLayouts.Add(
					new PipelineInputLayoutElementDesc
					{
						InputIndex = 5,
						Input = new InputLayoutElementDesc
						{
							BufferIndex = 1,
							ElementType = ElementType.Vector4,
							InstanceStepRate = 1
						}
					}
				);
			}

			desc.Samplers.Add(new ImmutableSamplerDesc
			{
				Name = "g_texture",
				Sampler = new SamplerStateDesc(TextureFilterMode.Trilinear, TextureAddressMode.Clamp)
			});

			return device.CreateGraphicsPipeline(desc);
		}

		private IShader LoadShader(IDevice device, ShaderType type, bool hasTexture, bool instanced)
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
			if(instanced)
			{
				shaderCI.Name = shaderCI.Name + "(INSTANCED)";
				shaderCI.Macros.Add("RENGINE_INSTANCED", "1");
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

		private void SetBinding(IShaderResourceBinding? binding, byte slot)
		{
			ITextureView? tex = pTextureManager.Textures[slot]?.GetDefaultView(TextureViewType.ShaderResource);
			if(tex != null)
				binding?.Set(ShaderTypeFlags.Pixel, "g_texture", tex);
		}
		private void SetCBufferBinding(IShaderResourceBinding? binding)
		{
			if (binding is null || pCBuffer is null)
				return;

			binding.Set(ShaderTypeFlags.Vertex, "Constants", pCBuffer);
		}
	}
}
