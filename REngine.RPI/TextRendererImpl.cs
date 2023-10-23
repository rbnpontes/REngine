using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Utils;
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
		class FontEntry : IDisposable
		{
			private bool pDisposed = false;

			public IShaderResourceBinding SRB { get; private set; }
			public Font Font { get; private set; }
			public ITexture TextureAtlas { get; private set; }

			public FontEntry(Font font, ITexture textureAtlas, IShaderResourceBinding srb)
			{
				Font = font;
				TextureAtlas = textureAtlas;
				SRB = srb;
			}

			public void Dispose()
			{
				if(pDisposed) return;

				SRB.Dispose();
				TextureAtlas.Dispose();

				pDisposed = true;
				GC.SuppressFinalize(this);
			}
		}
		class InternalBatch : TextRendererBatch
		{
			private readonly TextRendererImpl pRenderer;

			public LinkedListNode<InternalBatch>? BatchNode;

			public InternalBatch(
				TextRendererImpl renderer,
				IDevice device, 
				IPipelineState fontPipeline,
				IShaderResourceBinding srb,
				IBuffer cbuffer, 
				Font font
			) : base(device, fontPipeline, srb, cbuffer, font)
			{
				pRenderer = renderer;
			}

			protected override void OnDispose()
			{
				lock (pRenderer.pSync)
				{
					if(BatchNode != null)
						pRenderer.pBatches.Remove(BatchNode);
					BatchNode = null;
				}
			}
		}

		private readonly IBufferProvider pBufferProvider;
		private readonly ILogger<ITextRenderer> pLogger;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly EngineEvents pEngineEvents;
		private readonly IRenderer pRenderer;

		private readonly LinkedList<InternalBatch> pBatches = new();
		private readonly Dictionary<int, FontEntry> pFonts = new();

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

			if (pBatches.Count > 0)
				pLogger.Info($"Clearing ({pBatches.Count}) batches");
			DisposeBatches();

			if (pFonts.Count > 0)
				pLogger.Info($"Clearing ({pFonts.Count}) font textures");
			DisposeFonts();

			pPipeline?.Dispose();
			pPipeline = null;

			pEngineEvents.OnStop -= HandleEngineStop;

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void DisposeBatches()
		{
			while(pBatches.First != null)
				pBatches.First.Value.Dispose();
		}

		private void DisposeFonts()
		{
			lock (pSync)
			{
				foreach(var pair in pFonts)
					pair.Value.Dispose();
				pFonts.Clear();
			}
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private ITexture AllocateTexture(Font font, Image image)
		{
			var texture = GetDevice().CreateTexture(new TextureDesc
			{
				Name = $"Font ({font.Name}) Texture",
				AccessFlags = CpuAccessFlags.None,
				Size = new TextureSize(image.Size.Width, image.Size.Height),
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Default,
				Dimension = TextureDimension.Tex2D,
				Format = TextureFormat.R8UNorm
			}, new ITextureData[] { 
				new ByteTextureData(image.Data, image.Stride) 
			});

			return texture;
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

			for(uint i =0; i < 2; ++i)
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
					Sampler = new SamplerStateDesc(TextureFilterMode.Anisotropic, TextureAddressMode.Clamp)
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

		public ITextRenderer SetFont(Font font)
		{
			return SetFont(font, font.Name);
		}
		
		public ITextRenderer SetFont(Font font, string fontName)
		{
			if(string.IsNullOrEmpty(fontName))
				throw new ArgumentNullException(nameof(fontName));

			int fontHashCode = fontName.GetHashCode();
			FontEntry? fontEntry;
			lock(pSync)
				pFonts.TryGetValue(fontHashCode, out fontEntry);
			
			if (fontEntry?.Font == font)
				return this;

			SdfBuilder builder = new SdfBuilder(font.Atlas);
			builder.Radius = 4;
			builder.Cutoff = 0.45f;
			ITexture texture = AllocateTexture(font, builder.Build());

			IShaderResourceBinding srb;
			lock (pSync)
			{
				if (pPipeline is null)
					pPipeline = BuildPipeline();

				srb = pPipeline.CreateResourceBinding();
			}

			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Fixed, pBufferProvider.GetBuffer(BufferGroupType.Fixed));
			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Object, pBufferProvider.GetBuffer(BufferGroupType.Object));
			srb.Set(ShaderTypeFlags.Pixel, "g_texture", texture.GetDefaultView(TextureViewType.ShaderResource));

			lock(pSync)
				pFonts[fontHashCode] = new FontEntry(font, texture, srb);
			return this;
		}

		public TextRendererBatch CreateBatch(string fontName)
		{
			if (string.IsNullOrEmpty(fontName))
				throw new ArgumentNullException("font name cannot be null or empty. Font must register first with SetFont method.");

			if (pPipeline is null)
				pPipeline = BuildPipeline();

			FontEntry? fontEntry;
			pFonts.TryGetValue(fontName.GetHashCode(), out fontEntry);

			if (fontEntry is null)
				throw new NullReferenceException($"Font '{fontName}' was not found. Did you call SetFont method first ?");

			InternalBatch batch = new InternalBatch(
				this,
				pDevice,
				pPipeline,
				fontEntry.SRB,
				pBufferProvider.GetBuffer(BufferGroupType.Object),
				fontEntry.Font
			);
			return batch;
		}

		private IDevice GetDevice()
		{
			if (pDevice != null)
				return pDevice;
			pDevice = pRenderer.Driver?.Device;
			if (pDevice is null)
				throw new TextRendererException("Empty Graphics Driver. It seems renderer was not initialized");
			return pDevice;
		}
	}
}
