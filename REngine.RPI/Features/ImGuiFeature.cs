using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Constants;
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
    internal class ImGuiFeature : GraphicsRenderFeature
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
			GraphicsSettings settings
		) : base()
		{
			pSystem = system;
			pSettings = settings;
		}

		protected override void OnDispose()
		{
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

		protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
		{
			var buffer = setupInfo.BufferManager.GetBuffer(BufferGroupType.Frame);

			if(pFontTexture is null || (pDirtyFlags & DirtyFlags.FontTexture) != 0)
			{
				pFontTexture?.Dispose();
				
				var fontTexture = CreateFontTexture(setupInfo.Driver.Device);
				pFontTexture = fontTexture;
				pFontTextureView = fontTexture.GetDefaultView(TextureViewType.ShaderResource);
				
				pDirtyFlags ^= DirtyFlags.FontTexture;
			}

			if(pPipeline is null || (pDirtyFlags & DirtyFlags.Pipeline) != 0)
			{
				pResourceBinding?.Dispose();
				pPipeline?.Dispose();

				var pipeline = CreatePipelineState(setupInfo.PipelineStateManager, setupInfo.ShaderManager);
				pResourceBinding = pipeline.GetResourceBinding();
				pPipeline = pipeline;

				pResourceBinding.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Frame, buffer);
				pResourceBinding.Set(ShaderTypeFlags.Pixel, sFontTexName, pFontTextureView);

				pDirtyFlags ^= DirtyFlags.Pipeline;
			}

			pDevice = setupInfo.Driver.Device;
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			ITextureView? backbuffer = GetBackBuffer();
			ITextureView? depthbuffer = GetDepthBuffer();

			if (backbuffer is null || depthbuffer is null || pDevice is null)
				return;

			pSystem.BeginRender();
			var io = ImGuiNET.ImGui.GetIO();

			command.SetRT(backbuffer, depthbuffer);

			RenderDrawData(command, pDevice, io, backbuffer.Parent.Desc.Size);
			pSystem.EndRender();
		}

		private unsafe void RenderDrawData(ICommandBuffer command, IDevice device, ImGuiNET.ImGuiIOPtr io, in TextureSize texSize)
		{
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

					SetupRenderState(command);

					scissor.Left = (int)cmd.ClipRect.X;
					scissor.Top = (int)cmd.ClipRect.Y;
					scissor.Right = (int)cmd.ClipRect.Z;
					scissor.Bottom = (int)cmd.ClipRect.W;

					command
						.SetScissor(scissor, texSize.Width, texSize.Height)
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

		private void SetupRenderState(ICommandBuffer command)
		{
			if (pVBuffer is null || pIBuffer is null)
				return;
			command
				.SetVertexBuffer(pVBuffer)
				.SetIndexBuffer(pIBuffer)
				.SetPipeline(pPipeline)
				.SetBlendFactors(Color.Black);
		}

		private IPipelineState CreatePipelineState(IPipelineStateManager pipelineMgr, IShaderManager shaderMgr)
		{
			IShader vs = CreateShader(shaderMgr, ShaderType.Vertex);
			IShader ps = CreateShader(shaderMgr, ShaderType.Pixel);

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

			var pipeline = pipelineMgr.GetOrCreate(ci);

			return pipeline;
		}
		
		private IShader CreateShader(IShaderManager shaderMgr, ShaderType shaderType)
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

			return shaderMgr.GetOrCreate(new ShaderCreateInfo
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
