using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Resources;

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
				IBuffer constantBuffer, 
				Font font
			) : base(device, fontPipeline, srb, constantBuffer, font)
			{
				pRenderer = renderer;
			}

			protected override void OnDispose()
			{
				pRenderer.pBatchGroup.Lock();
				lock (pRenderer.pSync)
				{
					pRenderer.pBatchGroup.RemoveBatch(this);
					if(BatchNode != null)
						pRenderer.pBatches.Remove(BatchNode);
					BatchNode = null;
				}
				pRenderer.pBatchGroup.Unlock();
			}
		}

		private readonly IBufferManager pBufferProvider;
		private readonly ILogger<ITextRenderer> pLogger;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly EngineEvents pEngineEvents;
		private readonly IRenderer pRenderer;
		private readonly IAssetManager pAssetManager;
		private readonly BatchGroup pBatchGroup;

		private readonly LinkedList<InternalBatch> pBatches = new();
		private readonly Dictionary<ulong, FontEntry> pFonts = new();

		private readonly object pSync = new();

		private bool pDisposed;
		private GraphicsBackend pBackend;

		private IDevice? pDevice;
		private IPipelineState? pPipeline;

		public TextRendererImpl(
			IBufferManager bufferProvider,
			ILoggerFactory loggerFactory,
			GraphicsSettings graphicsSettings,
			EngineEvents engineEvents,
			IRenderer renderer,
			IAssetManager assetManager,
			BatchSystem batchSystem
		) 
		{
			pBufferProvider = bufferProvider;
			pLogger = loggerFactory.Build<ITextRenderer>();
			pGraphicsSettings = graphicsSettings;
			pEngineEvents = engineEvents;
			pRenderer = renderer;
			pAssetManager = assetManager;
			pBatchGroup = batchSystem.GetGroup(SpriteSystem.BatchGroupName);
			
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
			var backend = GetBackend();
			if (backend == GraphicsBackend.OpenGL)
			{
				var tmp = image;
				image = new Image();
				image.SetData(new ImageDataInfo
				{
					Data = new byte[tmp.Size.Width * tmp.Size.Height * 4],
					Components = 4,
					Size = tmp.Size
				});

				for (ushort x = 0; x < tmp.Size.Width; ++x)
				for (ushort y = 0; y < tmp.Size.Height; ++y)
					image.SetPixel(tmp.GetPixel(x, y), x, y);
			}
			
			var texture = GetDevice().CreateTexture(new TextureDesc
			{
				Name = $"Font ({font.Name}) Texture",
				AccessFlags = CpuAccessFlags.None,
				Size = new TextureSize(image.Size.Width, image.Size.Height),
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Default,
				Dimension = TextureDimension.Tex2D,
				Format = backend == GraphicsBackend.OpenGL ? TextureFormat.RGBA8UNorm : TextureFormat.R8UNorm,
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
					Name = TextureNames.MainTexture,
					Sampler = new SamplerStateDesc(TextureFilterMode.Anisotropic, TextureAddressMode.Clamp)
				}
			);

			var pipeline = GetDevice().CreateGraphicsPipeline(desc);

			vsShader.Dispose();
			psShader.Dispose();
			return pipeline;
		}

		private IShader LoadShader(ShaderType shaderType)
		{
			var shaderCI = new ShaderCreateInfo
			{
				Type = shaderType
			};

			ShaderAsset asset;
			switch (shaderType)
			{
				case ShaderType.Vertex:
					{
						shaderCI.Name = "Text Renderer Vertex Shader";
						asset = pAssetManager.GetAsset<ShaderAsset>("Shaders/text_vs.hlsl");
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Text Renderer Pixel Shader";
						asset = pAssetManager.GetAsset<ShaderAsset>("Shaders/text_ps.hlsl");
					}
					break;
				case ShaderType.Compute:
				case ShaderType.Geometry:
				case ShaderType.Hull:
				case ShaderType.Domain:
				default:
					throw new NotImplementedException();
			}

			shaderCI.SourceCode = asset.ShaderCode;
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

			var fontHashCode = Hash.Digest(fontName);
			lock (pSync)
			{
				pFonts.TryGetValue(fontHashCode, out var fontEntry);

				if (fontEntry?.Font == font)
					return this;
				fontEntry?.Dispose();

				var builder = new SdfBuilder(font.Atlas)
				{
					Radius = 4,
					Cutoff = 0.45f
				};
				var texture = AllocateTexture(font, builder.Build());

				IShaderResourceBinding srb;
				lock (pSync)
				{
					pPipeline ??= BuildPipeline();

					srb = pPipeline.CreateResourceBinding();
				}

				srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Frame, pBufferProvider.GetBuffer(BufferGroupType.Frame));
				srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Object, pBufferProvider.GetBuffer(BufferGroupType.Object));
				srb.Set(ShaderTypeFlags.Pixel, TextureNames.MainTexture, texture.GetDefaultView(TextureViewType.ShaderResource));

				pFonts[fontHashCode] = new FontEntry(font.Optimize(), texture, srb);

				GC.Collect();
			}
			return this;
		}

		public TextRendererBatch CreateBatch(string fontName)
		{
			if (string.IsNullOrEmpty(fontName))
				throw new ArgumentNullException(nameof(fontName));

			pPipeline ??= BuildPipeline();

			pFonts.TryGetValue(Hash.Digest(fontName), out var fontEntry);

			if (fontEntry is null)
				throw new NullReferenceException($"Font '{fontName}' not found. Did you call SetFont method first ?");

			pBatchGroup.Lock();
			var batch = new InternalBatch(
				this,
				pDevice,
				pPipeline,
				fontEntry.SRB,
				pBufferProvider.GetBuffer(BufferGroupType.Object),
				fontEntry.Font
			);
			batch.BatchNode = pBatches.AddLast(batch);
			pBatchGroup.AddBatch(batch);
			pBatchGroup.Unlock();
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

		private GraphicsBackend GetBackend()
		{
			if (pBackend != GraphicsBackend.Unknow)
				return pBackend;
			var driver = pRenderer.Driver;
			if (driver is null)
				return pBackend;
			return pBackend = driver.Backend;
		}
	}
}
