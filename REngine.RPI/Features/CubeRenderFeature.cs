﻿using REngine.Core.WorldManagement;
using REngine.RHI;
using REngine.RPI.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RPI.Resources;

namespace REngine.RPI.Features
{
    public interface ICubeRenderFeature : IGraphicsRenderFeature 
	{ 
		public Transform? Transform { get; set; }
		public Camera? Camera { get; set; }
	}

	internal class CubeRenderFeature : GraphicsRenderFeature, ICubeRenderFeature
	{
		protected GraphicsSettings pSettings;
		protected GraphicsPipelineDesc pPipelineDesc = new GraphicsPipelineDesc { Name = "Cube Pipeline" };
		protected BufferDesc pVertexBufferDesc = new BufferDesc { 
			Name = "VBuffer",
			Usage = Usage.Immutable,
			BindFlags = BindFlags.VertexBuffer,
		};
		protected BufferDesc pIndexBufferDesc = new BufferDesc { 
			Name = "IBuffer",
			Usage = Usage.Immutable,
			BindFlags = BindFlags.IndexBuffer
		};

		protected IBuffer? pVertexBuffer;
		protected IBuffer? pIndexBuffer;
		protected IBuffer? pObjectCBuffer;
		protected IBuffer? pCamCBuffer;
		protected IPipelineState? pPipeline;
		protected ISwapChain? pSwapChain;


		protected readonly static Vector3[] pVertices = new Vector3[]
		{
			new Vector3(-1, -1, -1),
			new Vector3(-1, +1, -1),
			new Vector3(+1, +1, -1),
			new Vector3(+1, -1, -1),

			new Vector3(-1, -1, +1),
			new Vector3(-1, +1, +1),
			new Vector3(+1, +1, +1),
			new Vector3(+1, -1, +1),
		};

		protected readonly static uint[] pIndices = new uint[]
		{
			2,0,1, 2,3,0,
			4,6,5, 4,7,6,
			0,7,4, 0,3,7,
			1,0,4, 1,4,5,
			1,5,2, 5,6,2,
			3,6,7, 3,2,6
		};

		protected readonly static Vector4[] pColors = new Vector4[]
		{
			new Vector4(1, 0, 0, 1),
			new Vector4(0, 1, 0, 1),
			new Vector4(0, 0, 1, 1),
			new Vector4(1, 1, 1, 1),
			
			new Vector4(1, 1, 0, 1),
			new Vector4(0, 1, 1, 1),
			new Vector4(1, 0, 1, 1),
			new Vector4(0.2f, 0.2f, 0.2f, 0.2f)
		};

		public override bool IsDirty { get; protected set; } = true;

		public Transform? Transform { get; set; }
		public Camera? Camera { get; set; }

		public CubeRenderFeature(GraphicsSettings settings) : base()
		{
			pSettings = settings;
		}

		protected override void OnDispose()
		{
			pVertexBuffer?.Dispose();
			pIndexBuffer?.Dispose();

			pVertexBuffer = pIndexBuffer = null;
			pPipeline = null;

			base.OnDispose();
		}

		public override IRenderFeature MarkAsDirty()
		{
			IsDirty = true;
			return this;
		}

		protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
		{
			pVertexBuffer?.Dispose();
			pIndexBuffer?.Dispose();
			pPipeline?.Dispose();

			pVertexBuffer = CreateVertexBuffer(setupInfo.Driver.Device);
			pIndexBuffer = CreateIndexBuffer(setupInfo.Driver.Device);
			pPipeline = CreatePipeline(
				setupInfo.PipelineStateManager, 
				setupInfo.ShaderManager, 
				setupInfo.BufferManager,
				setupInfo.AssetManager);
			pSwapChain = setupInfo.Renderer.SwapChain;
			pObjectCBuffer = setupInfo.BufferManager.GetBuffer(BufferGroupType.Object);
			pCamCBuffer = setupInfo.BufferManager.GetBuffer(BufferGroupType.Camera);

			IsDirty = false;
		}

		protected override void OnExecute(ICommandBuffer command)
		{
#if PROFILER
			using var _ = Profiler.Instance.Begin();
#endif
			ITextureView? backbuffer = GetBackBuffer();
			ITextureView? depthbuffer = GetDepthBuffer();

			if (Camera is null || Transform is null || backbuffer is null || depthbuffer is null)
				return;
			if (pVertexBuffer is null || pIndexBuffer is null || pObjectCBuffer is null || pPipeline is null)
				return;

			Camera.GetCBufferData(out var camCBufferData);

			UpdateCBuffer(command, pObjectCBuffer, Transform.WorldTransformMatrix);
			UpdateCBuffer(command, pCamCBuffer, camCBufferData);

			command
				.SetRT(backbuffer, depthbuffer)
				.SetVertexBuffers(0, new IBuffer[] { pVertexBuffer, pVertexBuffer }, new ulong[] { 0, (ulong)(Marshal.SizeOf<Vector3>() * pVertices.Length)})
				.SetIndexBuffer(pIndexBuffer)
				.SetPipeline(pPipeline)
				.CommitBindings(pPipeline.GetResourceBinding())
				.Draw(new DrawIndexedArgs
				{
					NumIndices = (uint)pIndices.Length
				});
		}

		private void UpdateCBuffer<T>(ICommandBuffer command, IBuffer buffer, in T data) where T : unmanaged
		{
			var mappedData = command.Map<T>(buffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = data;
			command.Unmap(buffer, MapType.Write);
		}

		protected virtual IShader LoadShader(IShaderManager shaderMgr, ShaderType type, IAssetManager assetManager)
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
						shaderCI.Name = "Cube Vertex Shader";
						shaderCI.SourceCode = assetManager.GetAsset<ShaderAsset>("Shaders/cube_feat_vs.hlsl").ShaderCode;
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Cube Pixel Shader";
						shaderCI.SourceCode = assetManager.GetAsset<ShaderAsset>("Shaders/cube_feat_ps.hlsl").ShaderCode;
					}
					break;
				default:
					throw new NotImplementedException();
			}
			
			return shaderMgr.GetOrCreate(shaderCI);
		}

		protected virtual IPipelineState CreatePipeline(
			IPipelineStateManager pipelineMgr, 
			IShaderManager shaderMgr, 
			IBufferManager bufferMgr, 
			IAssetManager assetManager)
		{
			IShader vsShader = LoadShader(shaderMgr, ShaderType.Vertex, assetManager);
			IShader psShader = LoadShader(shaderMgr, ShaderType.Pixel, assetManager);

			pPipelineDesc.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			pPipelineDesc.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;
			pPipelineDesc.BlendState.BlendMode = BlendMode.Replace;
			pPipelineDesc.PrimitiveType = PrimitiveType.TriangleList;
			pPipelineDesc.RasterizerState.CullMode = CullMode.Back;
			pPipelineDesc.DepthStencilState.EnableDepth = true;

			pPipelineDesc.Shaders.VertexShader = vsShader;
			pPipelineDesc.Shaders.PixelShader = psShader;

			SetupInputLayout(pPipelineDesc.InputLayouts);

			var pipeline = pipelineMgr.GetOrCreate(pPipelineDesc);

			var srb = pipeline.GetResourceBinding();
			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Camera, bufferMgr.GetBuffer(BufferGroupType.Camera));
			srb.Set(ShaderTypeFlags.Vertex, ConstantBufferNames.Object, bufferMgr.GetBuffer(BufferGroupType.Object));

			pPipelineDesc.Shaders.VertexShader = pPipelineDesc.Shaders.PixelShader = null;

			return pipeline;
		}
		protected virtual void SetupInputLayout(IList<PipelineInputLayoutElementDesc> inputLayout)
		{
			inputLayout.Clear();
			inputLayout.Add(
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 0,
					Input = new InputLayoutElementDesc
					{
						BufferIndex = 0,
						ElementType = ElementType.Vector3
					}
				}
			);
			inputLayout.Add(new PipelineInputLayoutElementDesc
			{
				InputIndex = 1,
				Input = new InputLayoutElementDesc
				{
					ElementType = ElementType.Vector4,
					BufferIndex = 1
				}
			});
		}
	
		protected virtual IBuffer CreateVertexBuffer(IDevice device)
		{
			var verticesSize = Marshal.SizeOf<Vector3>() * pVertices.Length;
			var colorsSize = Marshal.SizeOf<Vector4>() * pColors.Length;

			byte[] buffer = new byte[verticesSize + colorsSize];
			pVertexBufferDesc.Size = (uint)buffer.Length;

			var handle = GCHandle.Alloc(pVertices, GCHandleType.Pinned);
			Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, verticesSize);
			handle.Free();

			handle = GCHandle.Alloc(pColors, GCHandleType.Pinned);
			Marshal.Copy(handle.AddrOfPinnedObject(), buffer, verticesSize, colorsSize);
			handle.Free();
			

			var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IBuffer result = device.CreateBuffer(pVertexBufferDesc, bufferHandle.AddrOfPinnedObject(), (ulong)buffer.Length);
			bufferHandle.Free();

			return result;
		}
		protected virtual IBuffer CreateIndexBuffer(IDevice device)
		{
			pIndexBufferDesc.Size = (ulong)(Marshal.SizeOf<uint>() * pIndices.Length);
			return device.CreateBuffer(pIndexBufferDesc, pIndices);
		}
	}
}
