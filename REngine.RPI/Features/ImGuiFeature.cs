using REngine.Core.Mathematics;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	internal class ImGuiFeature : BaseRenderFeature
	{
		[Flags]
		public enum DirtyFlags
		{
			None =0,
			Pipeline = 1 << 0,
			FontTexture = 1 << 1,
			All = Pipeline | FontTexture
		}

		private static readonly ulong sImDrawVertSize = (ulong)Marshal.SizeOf<ImGuiNET.ImDrawVert>();
		private static readonly string sFontTexName = "g_texture";
		private static readonly List<PipelineInputLayoutElementDesc> sLayoutElements = new()
		{
			new PipelineInputLayoutElementDesc 
			{
				InputIndex = 0,
				Input = new InputLayoutElementDesc
				{
					BufferIndex = 0,
					ElementType = ElementType.Vector2
				}
			},
			new PipelineInputLayoutElementDesc
			{
				InputIndex = 1,
				Input = new InputLayoutElementDesc
				{
					BufferIndex = 0,
					ElementType = ElementType.Vector2
				}
			},
			new PipelineInputLayoutElementDesc
			{
				InputIndex = 2,
				Input = new InputLayoutElementDesc
				{
					BufferIndex =0,
					ElementType = ElementType.UByte4,
					IsNormalized = true
				}
			}
		};
		private static readonly List<ImmutableSamplerDesc> sImmutableSamplers = new()
		{
			new ImmutableSamplerDesc
			{
				Name = sFontTexName,
				Sampler = new SamplerStateDesc(TextureFilterMode.Bilinear)
			}
		};

		private readonly ImGuiSystem pSystem;
		private readonly GraphicsSettings pSettings;
		private IRenderer pRenderer;

		private IBuffer? pCBuffer;
		private ITexture? pFontTexture;
		private ITextureView? pFontTextureView;
		private IPipelineState? pPipeline;
		private IShaderResourceBinding? pResourceBinding;

		private IBuffer? pVBuffer;
		private IBuffer? pIBuffer;

		private IDevice? pDevice;

		private DirtyFlags pDirtyFlags = DirtyFlags.All;

		private uint pVertexBufferCount = 0;
		private uint pIndexBufferCount = 0;

		public override bool IsDirty { get => pDirtyFlags != DirtyFlags.None; protected set { } }

		public ImGuiFeature(
			ImGuiSystem system, 
			GraphicsSettings settings,
			IRenderer renderer) : base()
		{
			pSystem = system;
			pSettings = settings;
			pRenderer = renderer;
		}

		protected override void OnDispose()
		{
			pResourceBinding?.Dispose();
			pPipeline?.Dispose();
			pFontTexture?.Dispose();

			pVBuffer?.Dispose();
			pIBuffer?.Dispose();

			pResourceBinding = null;
			pPipeline = null;
			pFontTexture = null;
			pFontTextureView = null;
			pPipeline = null;

			pVBuffer = pIBuffer = null;

			base.OnDispose();
		}
		
		public override IRenderFeature MarkAsDirty()
		{
			return MarkAsDirty(DirtyFlags.All);
		}
		public IRenderFeature MarkAsDirty(DirtyFlags flags)
		{
			pDirtyFlags |= flags;
			return this;
		}

		protected override void OnSetup(in RenderFeatureSetupInfo execInfo)
		{
			var buffer = execInfo.BufferProvider.GetBuffer(BufferGroupType.Object);
			if(buffer != pCBuffer)
			{
				pResourceBinding?.Set(ShaderTypeFlags.Vertex, "Constants", buffer);
				pCBuffer = buffer;
			}

			if(pFontTexture is null || (pDirtyFlags & DirtyFlags.FontTexture) != 0)
			{
				pFontTexture?.Dispose();
				
				var fontTexture = CreateFontTexture(execInfo.Driver.Device);
				pFontTexture = fontTexture;
				pFontTextureView = fontTexture.GetDefaultView(TextureViewType.ShaderResource);
				
				pDirtyFlags ^= DirtyFlags.FontTexture;
			}

			if(pPipeline is null || (pDirtyFlags & DirtyFlags.Pipeline) != 0)
			{
				pResourceBinding?.Dispose();
				pPipeline?.Dispose();

				var pipeline = CreatePipelineState(execInfo.Driver.Device);
				pResourceBinding = pipeline.GetResourceBinding();
				pPipeline = pipeline;

				pResourceBinding.Set(ShaderTypeFlags.Vertex, "Constants", pCBuffer);
				pResourceBinding.Set(ShaderTypeFlags.Pixel, sFontTexName, pFontTextureView);

				pDirtyFlags ^= DirtyFlags.Pipeline;
			}

			pDevice = execInfo.Driver.Device;
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			var swapChain = pRenderer.SwapChain;
			if (swapChain is null || pDevice is null)
				return;

			pSystem.BeginRender();
			var io = ImGuiNET.ImGui.GetIO();

			CreateProjection(swapChain.Size, out Matrix4x4 output);

			var mappedData = command.Map<Matrix4x4>(pCBuffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = output;
			command.Unmap(pCBuffer, MapType.Write);

			command.SetRT(swapChain.ColorBuffer, swapChain.DepthBuffer);

			RenderDrawData(command, pDevice, io, swapChain);
			pSystem.EndRender();
		}

		private unsafe void RenderDrawData(ICommandBuffer command, IDevice device, ImGuiNET.ImGuiIOPtr io, ISwapChain swapChain)
		{
			Viewport viewport = new Viewport
			{
				Size = new Vector2(swapChain.Size.Width, swapChain.Size.Height),
			};
			IntRect scissor = new();

			var drawData = ImGuiNET.ImGui.GetDrawData();

			if (drawData.NativePtr == IntPtr.Zero.ToPointer())
				return;

			drawData.ScaleClipRects(io.DisplayFramebufferScale);

			for (int n =0; n < drawData.CmdListsCount; ++n)
			{
				ImGuiNET.ImDrawListPtr cmdList = drawData.CmdLists[n];

				if(cmdList.VtxBuffer.Size > pVertexBufferCount)
				{
					pVertexBufferCount = (uint)cmdList.VtxBuffer.Size * 2;
					ResizeVBuffer(device);
				}

				if(cmdList.IdxBuffer.Size > pIndexBufferCount) 
				{
					pIndexBufferCount = (uint)cmdList.IdxBuffer.Size * 2;
					ResizeIBuffer(device);
				}

				UpdateMeshBuffers(cmdList, command);

				uint idxBufferOffset = 0;
				for(int cmdIdx =0; cmdIdx < cmdList.CmdBuffer.Size; ++cmdIdx)
				{
					ImGuiNET.ImDrawCmdPtr cmd = cmdList.CmdBuffer[cmdIdx];

					if (cmd.UserCallback != IntPtr.Zero)
						continue;

					SetupRenderState(command, viewport, swapChain.Size);

					scissor.Left = (int)cmd.ClipRect.X;
					scissor.Top = (int)cmd.ClipRect.Y;
					scissor.Right = (int)cmd.ClipRect.Z;
					scissor.Bottom = (int)cmd.ClipRect.W;

					command
						.SetScissor(scissor, swapChain.Size.Width, swapChain.Size.Height)
						.CommitBindings(pResourceBinding)
						.Draw(new DrawIndexedArgs
						{
							FirstIndexLocation = idxBufferOffset,
							NumIndices = cmd.ElemCount,
							BaseVertex = 0,
							IndexType = RHI.ValueType.UInt16
						});

					idxBufferOffset += cmd.ElemCount;
				}
			}
		}

		private void CreateProjection(in SwapChainSize size, out Matrix4x4 output)
		{
			Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0.0f, 1.0f);
			projection.M33 = projection.M43 = 0.5f;
			output = projection;
		}

		private void SetupRenderState(ICommandBuffer command, in Viewport viewport, in SwapChainSize size)
		{
			if (pVBuffer is null || pIBuffer is null)
				return;
			command
				.SetVertexBuffer(pVBuffer)
				.SetIndexBuffer(pIBuffer)
				.SetPipeline(pPipeline)
				.SetBlendFactors(Color.Black)
				.SetViewport(viewport, size.Width, size.Height);
		}

		private IPipelineState CreatePipelineState(IDevice device)
		{
			IShader vs = CreateShader(device, ShaderType.Vertex);
			IShader ps = CreateShader(device, ShaderType.Pixel);

			GraphicsPipelineDesc ci = new();
			ci.Name = "ImGui PSO";

			ci.PrimitiveType = PrimitiveType.TriangleList;
			ci.BlendState.BlendMode = BlendMode.Alpha;
			ci.DepthStencilState.DepthCompareFunction = CompareMode.Always;
			ci.DepthStencilState.DepthWriteEnabled = false;
			ci.RasterizerState.ScissorTestEnabled = true;

			ci.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			ci.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;

			ci.InputLayouts = sLayoutElements;
			ci.Samplers = sImmutableSamplers;

			ci.Shaders.VertexShader = vs;
			ci.Shaders.PixelShader = ps;

			var pipeline = device.CreateGraphicsPipeline(ci);

			vs.Dispose();
			ps.Dispose();

			return pipeline;
		}
		
		private IShader CreateShader(IDevice device, ShaderType shaderType)
		{
			string shaderPath = Path.Join(
				AppDomain.CurrentDomain.BaseDirectory,
				"Assets/Shaders"
			);
			switch (shaderType)
			{
				case ShaderType.Vertex:
					shaderPath = Path.Join(shaderPath, "imgui_vs.hlsl");
					break;
				case ShaderType.Pixel:
					shaderPath = Path.Join(shaderPath, "imgui_ps.hlsl");
					break;
				default:
					throw new NotSupportedException();
			}

			return device.CreateShader(new ShaderCreateInfo
			{
				Type = shaderType,
				SourceCode = File.ReadAllText(shaderPath),
				Name = $"ImGui Shader ({shaderType})"
			});
		}

		private ITexture CreateFontTexture(IDevice device)
		{
			var fontSize = pSystem.FontSize;
			TextureDesc desc = new TextureDesc
			{
				Name = "ImGui Font Texture",
				Dimension = TextureDimension.Tex2D,
				Size = new TextureSize(fontSize.Width, fontSize.Height),
				Format = TextureFormat.RGBA8UNorm,
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable
			};
			return device.CreateTexture(desc, new ITextureData[]
			{
				new ByteTextureData(pSystem.FontData, 4 * desc.Size.Width)
			});
		}
	
		private void ResizeIBuffer(IDevice device)
		{
			pIBuffer?.Dispose();
			pIBuffer = CreateIBuffer(device);
		}
		
		private void ResizeVBuffer(IDevice device)
		{
			pVBuffer?.Dispose();
			pVBuffer = CreateVBuffer(device);
		}

		private unsafe void UpdateMeshBuffers(ImGuiNET.ImDrawListPtr cmdList, ICommandBuffer cmd)
		{
			if(pIBuffer is null || pVBuffer is null) 
				return;

			IntPtr vertexMemPtr = cmd.Map(pVBuffer, MapType.Write, MapFlags.Discard);
			IntPtr indexMemPtr = cmd.Map(pIBuffer, MapType.Write, MapFlags.Discard);

			long vBufferSize = cmdList.VtxBuffer.Size * (long)sImDrawVertSize;
			long iBufferSize = cmdList.IdxBuffer.Size * sizeof(ushort);

			Buffer.MemoryCopy(
				cmdList.VtxBuffer.Data.ToPointer(), vertexMemPtr.ToPointer(),
				vBufferSize, vBufferSize
			);
			Buffer.MemoryCopy(
				cmdList.IdxBuffer.Data.ToPointer(), indexMemPtr.ToPointer(),
				iBufferSize, iBufferSize
			);

			cmd.Unmap(pIBuffer, MapType.Write);
			cmd.Unmap(pVBuffer, MapType.Write);
		}

		private IBuffer CreateIBuffer(IDevice device)
		{
			return device.CreateBuffer(new BufferDesc
			{
				Name = "ImGui Index Buffer",
				BindFlags = BindFlags.IndexBuffer,
				Size = pIndexBufferCount * sizeof(short),
				Usage = Usage.Dynamic,
				AccessFlags = CpuAccessFlags.Write
			});
		}

		private IBuffer CreateVBuffer(IDevice device)
		{
			return device.CreateBuffer(new BufferDesc
			{
				Name = "ImGui Vertex Buffer",
				BindFlags = BindFlags.VertexBuffer,
				Size = pVertexBufferCount * sImDrawVertSize,
				Usage = Usage.Dynamic,
				AccessFlags = CpuAccessFlags.Write
			});
		}
	}
}
