using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class TextRendererImpl : ITextRenderer, IDisposable
	{
		struct Vertex
		{
			public Vector2 Position;
			public Vector2 UV;
			public Vector4 Color;
		}

		struct TextChar
		{
			public Vector4 Color;
			public Vector4 Bounds;
			public Vector4 PositionAndAtlasSize;
		}

		class TextRendererBatchImpl : TextRendererBatch
		{
			private readonly TextRendererImpl pRenderer;

			private bool pDisposed;

			public LinkedListNode<TextRendererBatchImpl>? TargetNode;
		
			public override IPipelineState PipelineState { get; }

			public override IShaderResourceBinding ShaderResourceBinding { get; }

			public override IBuffer VertexBuffer { get; }

			public override ITexture FontTexture { get; }

			public override uint NumItems { get; }

			public TextRendererBatchImpl(
				TextRendererImpl renderer,
				IPipelineState pipeline,
				IShaderResourceBinding binding,
				IBuffer vbuffer,
				ITexture fontTexture,
				uint numItems
			)
			{
				pRenderer = renderer;
				ShaderResourceBinding = binding;
				PipelineState = pipeline;
				VertexBuffer = vbuffer;
				FontTexture = fontTexture;
				NumItems = numItems;
			}

			public override void Dispose()
			{
				if (pDisposed)
					return;

				VertexBuffer.Dispose();
				ShaderResourceBinding.Dispose();

				if(TargetNode != null)
				{
					lock(pRenderer.pSync)
						pRenderer.pBatches2Dispose.Remove(TargetNode);
				}

				pDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		class BufferWrapper : IBuffer
		{
			private readonly IBuffer pBuffer;
			private readonly TextRendererImpl pRenderer;

			private bool pDisposed = false;

			public LinkedListNode<BufferWrapper>? TargetNode;

			public BufferDesc Desc { get => pBuffer.Desc; }

			public ulong Size { get => pBuffer.Size; }

			public string Name { get => pBuffer.Name; }

			public IntPtr Handle { get => pBuffer.Handle; }

			public bool IsDisposed { get => pDisposed; }

			public BufferWrapper(IBuffer buffer, TextRendererImpl renderer)
			{
				pBuffer = buffer;
				pRenderer = renderer;
			}

			public event EventHandler? OnDispose;

			public void Dispose()
			{
				if(pDisposed) return;

				pBuffer.Dispose();

				OnDispose?.Invoke(this, EventArgs.Empty);

				if (TargetNode != null)
				{
					lock (pRenderer.pSync)
						pRenderer.pBuffers2Dispose.Remove(TargetNode);
				}

				pDisposed = true;
				GC.SuppressFinalize(this);
			}
		}
		class TextureWrapper : ITexture
		{
			private readonly ITexture pTexture;
			private readonly TextRendererImpl pRenderer;
			private readonly Font pFont;

			private bool pDisposed = false;

			public TextureDesc Desc => pTexture.Desc;

			public string Name => pTexture.Name;

			public IntPtr Handle => pTexture.Handle;

			public bool IsDisposed => pDisposed;

			public event EventHandler? OnDispose;

			public TextureWrapper(ITexture texture, TextRendererImpl renderer, Font font)
			{
				pTexture = texture;
				pRenderer = renderer;
				pFont = font;
				texture.OnDispose += HandleDispose;
			}

			private void HandleDispose(object? sender, EventArgs e)
			{
				pTexture.OnDispose -= HandleDispose;
				OnDispose?.Invoke(this, EventArgs.Empty);
			}

			public void Dispose()
			{
				if(pDisposed) return;

				pTexture.Dispose();
				lock (pRenderer.pSync)
				{
					if(pRenderer.pFontTextures.ContainsKey(pFont.Name.GetHashCode()))
						pRenderer.pFontTextures.Remove(pFont.Name.GetHashCode());
				}

				pDisposed = true;
				GC.SuppressFinalize(this);
			}

			public ITextureView GetDefaultView(TextureViewType view)
			{
				return pTexture.GetDefaultView(view);
			}
		}

		private readonly IBufferProvider pBufferProvider;
		private readonly ILogger<ITextRenderer> pLogger;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly EngineEvents pEngineEvents;
		private readonly IRenderer pRenderer;

		private readonly LinkedList<TextRendererBatchImpl> pBatches2Dispose = new();
		private readonly LinkedList<BufferWrapper> pBuffers2Dispose = new();

		private readonly Dictionary<int, TextureWrapper> pFontTextures = new();

		private readonly object pSync = new();

		private bool pDisposed;

		private IDevice? pDevice;
		private IPipelineState? pPipeline;

		public TextRendererImpl(
			IBufferProvider bufferProvider,
			ILoggerFactory loggerFactory,
			GraphicsSettings graphicsSettings,
			EngineEvents engineEvents,
			IRenderer renderer
		) 
		{
			pBufferProvider = bufferProvider;
			pLogger = loggerFactory.Build<ITextRenderer>();
			pGraphicsSettings = graphicsSettings;
			pEngineEvents = engineEvents;
			pRenderer = renderer;

			engineEvents.OnStop += HandleEngineStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			if (pBatches2Dispose.Count > 0)
				pLogger.Info($"Clearing ({pBatches2Dispose.Count}) batches");
			DisposeItems(pBatches2Dispose);

			if (pBuffers2Dispose.Count > 0)
				pLogger.Info($"Clearing ({pBuffers2Dispose.Count}) buffers");
			DisposeItems(pBuffers2Dispose);

			if (pFontTextures.Count > 0)
				pLogger.Info($"Clearing ({pFontTextures.Count}) font textures");
			DisposeFontTextures();

			pPipeline?.Dispose();
			pPipeline = null;

			pEngineEvents.OnStop -= HandleEngineStop;

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void DisposeItems<T>(LinkedList<T> list) where T : class
		{
			LinkedListNode<T>? nextNode;
			lock(pSync)
				nextNode = list.First;
			while(nextNode != null)
			{
				var currNode = nextNode;
				lock(pSync)
					nextNode = currNode.Next;

				if(currNode.Value is IDisposable disposable)
					disposable.Dispose();
			}

			lock (pSync)
				list.Clear();
		}

		private void DisposeFontTextures()
		{
			List<KeyValuePair<int, TextureWrapper>> texPair;

			lock (pSync)
			{
				texPair = pFontTextures.ToList();
				pFontTextures.Clear();
			}

			foreach(var pair in texPair)
				pair.Value.Dispose();
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private BufferWrapper AllocateVBuffer(TextChar[] chars)
		{
			var buffer = GetDevice().CreateBuffer(new BufferDesc
			{
				Name = "Text Renderer Vertex Buffer",
				BindFlags = BindFlags.VertexBuffer,
				Usage = Usage.Immutable,
				Size = (ulong)(chars.Length * Marshal.SizeOf<TextChar>()),
			}, chars);
			return new BufferWrapper(buffer, this);
		}

		//private IBuffer AllocateIBuffer(uint[] indices)
		//{
		//	return pDevice.CreateBuffer(new BufferDesc
		//	{
		//		Name = "Text Renderer Index Buffer",
		//		BindFlags = BindFlags.IndexBuffer,
		//		Usage = Usage.Dynamic,
		//		AccessFlags = CpuAccessFlags.Write,
		//		Size = (ulong)(indices.Length * sizeof(uint))
		//	}, indices);
		//}

		private TextureWrapper AllocateTexture(Font font)
		{
			var texture = GetDevice().CreateTexture(new TextureDesc
			{
				Name = $"Font ({font.Name}) Texture",
				AccessFlags = CpuAccessFlags.None,
				Size = new TextureSize(font.Atlas.Size.Width, font.Atlas.Size.Height),
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable,
				Dimension = TextureDimension.Tex2D,
				Format = TextureFormat.R8UNorm
			}, new ITextureData[] { 
				new ByteTextureData(font.Atlas.Data, font.Atlas.Stride) 
			});

			return new TextureWrapper(texture, this, font);
		}

		private IPipelineState BuildPipeline()
		{
			IShader vsShader = LoadShader(ShaderType.Vertex);
			IShader psShader = LoadShader(ShaderType.Pixel);

			GraphicsPipelineDesc desc = new();
			desc.Name = "Text Renderer Pipeline";
			desc.Output.RenderTargetFormats[0] = pGraphicsSettings.DefaultColorFormat;
			desc.Output.DepthStencilFormat = pGraphicsSettings.DefaultDepthFormat;
			desc.PrimitiveType = PrimitiveType.TriangleStrip;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = false;
			desc.BlendState.BlendMode = BlendMode.Alpha;

			desc.Shaders.VertexShader = vsShader;
			desc.Shaders.PixelShader = psShader;

			for(uint i =0; i < 3; ++i)
			{
				desc.InputLayouts.Add(
					new PipelineInputLayoutElementDesc
					{
						InputIndex = i,
						Input = new InputLayoutElementDesc
						{
							BufferIndex = 0,
							ElementType = ElementType.Vector4,
							InstanceStepRate = 1
						}
					}
				);
			}

			desc.Samplers.Add(
				new ImmutableSamplerDesc
				{
					Name = "g_texture",
					Sampler = new SamplerStateDesc(TextureFilterMode.Nearest, TextureAddressMode.Clamp)
				}
			);

			var pipeline = GetDevice().CreateGraphicsPipeline(desc);
			return pipeline;
		}

		private IShader LoadShader(ShaderType shaderType)
		{
			string shaderPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Shaders");
			ShaderCreateInfo shaderCI = new ShaderCreateInfo
			{
				Type = shaderType
			};

			switch (shaderType)
			{
				case ShaderType.Vertex:
					{
						shaderCI.Name = "Text Renderer Vertex Shader";
						shaderPath = Path.Join(shaderPath, "text_vs.hlsl");
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Text Renderer Pixel Shader";
						shaderPath = Path.Join(shaderPath, "text_ps.hlsl");
					}
					break;
				default:
					throw new NotImplementedException();
			}

			shaderCI.SourceCode = File.ReadAllText(shaderPath);
			return GetDevice().CreateShader(shaderCI);
		}

		public ITexture? GetFontTexture(Font font)
		{
			return TryGetFontTexture(font);
		}

		private TextureWrapper? TryGetFontTexture(Font font)
		{
			TextureWrapper? tex;
			lock (pSync)
				pFontTextures.TryGetValue(font.Name.GetHashCode(), out tex);
			return tex;
		}

		public TextRendererBatch CreateBatch(TextRendererCreateInfo createInfo)
		{
			if (string.IsNullOrEmpty(createInfo.Text))
				throw new ArgumentNullException("text cannot be null or empty");

			if (pPipeline is null)
				pPipeline = BuildPipeline();

			TextChar[] chars = CreateChars(createInfo.Font, createInfo.Color, createInfo.Position, createInfo.Text);
			BufferWrapper vbuffer = AllocateVBuffer(chars);

			lock(pSync)
				vbuffer.TargetNode = pBuffers2Dispose.AddLast(vbuffer);

			TextureWrapper? texture = TryGetFontTexture(createInfo.Font);
			if(texture is null)
			{
				texture = AllocateTexture(createInfo.Font);
				lock(pSync)
					pFontTextures.Add(createInfo.Font.Name.GetHashCode(), texture);
			}

			var srb = pPipeline.CreateResourceBinding();
			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Fixed, pBufferProvider.GetBuffer(BufferGroupType.Fixed));
			srb.Set(ShaderTypeFlags.Pixel, "g_texture", texture.GetDefaultView(TextureViewType.ShaderResource));

			var result = new TextRendererBatchImpl(
				this,
				pPipeline,
				srb,
				vbuffer,
				texture,
				(uint)chars.Length
			);

			lock(pSync)
				result.TargetNode = pBatches2Dispose.AddLast(result);

			return result;
		}

		private TextChar[] CreateChars(Font font, in Color color, in Vector2 position, string text)
		{
			TextChar[] chars = new TextChar[text.Length];
			Vector4 charColor = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
		
			float baseX = position.X;

			for(int i = 0;  i < chars.Length; i++)
			{
				byte glyphIndex = font.GetGlyhIndex(text[i]);
				var bounds = font.GetBounds(glyphIndex);
				var offset = font.GetOffset(glyphIndex);
				var advance = font.GetAdvance(glyphIndex);

				chars[i] = new TextChar
				{
					PositionAndAtlasSize = new Vector4(
						baseX + offset.X, 
						(position.Y + font.CharSize.Height) - offset.Y,
						font.Atlas.Size.Width,
						font.Atlas.Size.Height
					),
					Bounds = bounds.ToVector4(),
					Color = charColor,
				};

				baseX += advance.X;
			}

			return chars;
		}

		//private Vertex[] CreateVertices(Font font, in Color color, in Vector2 position, string text)
		//{
		//	Vertex[] vertices = new Vertex[text.Length * 6];
		//	Vector4 vertexColor = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

		//	float baseX = position.X;
		//	for(int i =0; i < text.Length; ++i)
		//	{
		//		int vertexIdx = i * 6;
		//		byte glyphIndex = font.GetGlyhIndex(text[i]);
		//		var bounds = font.GetBounds(glyphIndex);
		//		var offset = font.GetOffset(glyphIndex);
		//		var advance = font.GetAdvance(glyphIndex);

		//		float x = baseX + offset.X;
		//		float y = bounds.Height - offset.Y;

		//		float w = bounds.Width;
		//		float h = bounds.Height;

		//		// UV must be in normalized values [0...1]
		//		Vector2 leftTopUv = new Vector2(bounds.Left / (float)font.Atlas.Size.Width, bounds.Top / (float)font.Atlas.Size.Height);
		//		Vector2 rightBottomUv = new Vector2(bounds.Right / (float)font.Atlas.Size.Width, bounds.Bottom / (float)font.Atlas.Size.Height);

		//		// First Triangle
		//		vertices[vertexIdx] = new Vertex
		//		{
		//			Position = new Vector2(x, y + h),
		//			UV = new Vector2(leftTopUv.X, leftTopUv.Y),
		//			Color = vertexColor
		//		};
		//		vertices[vertexIdx + 1] = new Vertex
		//		{
		//			Position = new Vector2(x, y),
		//			UV = new Vector2(leftTopUv.X, rightBottomUv.Y),
		//			Color = vertexColor
		//		};
		//		vertices[vertexIdx + 2] = new Vertex
		//		{
		//			Position = new Vector2(x + w, y),
		//			UV = new Vector2(rightBottomUv.X, rightBottomUv.Y),
		//			Color = vertexColor
		//		};
		//		// Second Triangle
		//		vertices[vertexIdx + 3] = new Vertex
		//		{
		//			Position = new Vector2(x, y + h),
		//			UV = new Vector2(leftTopUv.X, leftTopUv.Y),
		//			Color = vertexColor
		//		};
		//		vertices[vertexIdx + 4] = new Vertex
		//		{
		//			Position = new Vector2(x + w, y),
		//			UV = new Vector2(rightBottomUv.X, rightBottomUv.Y),
		//			Color = vertexColor
		//		};
		//		vertices[vertexIdx + 5] = new Vertex
		//		{
		//			Position = new Vector2(x + w, y + h),
		//			UV = new Vector2(rightBottomUv.X, leftTopUv.Y),
		//			Color = vertexColor
		//		};

		//		baseX += 16;
		//	}

		//	return vertices;
		//}

		private IDevice GetDevice()
		{
			if (pDevice != null)
				return pDevice;
			pDevice = pRenderer.Driver?.Device;
			if (pDevice is null)
				throw new TextRendererException("Empty Graphics Driver. It seems that Renderer was not initialized");
			return pDevice;
		}
	}
}
