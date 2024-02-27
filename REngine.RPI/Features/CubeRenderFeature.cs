using REngine.Core.WorldManagement;
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
using REngine.Core.Threading;
using REngine.RPI.Resources;

namespace REngine.RPI.Features
{
    public interface ICubeRenderFeature : IGraphicsRenderFeature 
	{ 
		public Transform? Transform { get; set; }
		public Camera? Camera { get; set; }
	}

	internal class CubeRenderFeature(
		GraphicsSettings settings,
		IRenderer renderer,
		IExecutionPipeline executionPipeline) : AsyncGraphicsRenderFeature(renderer, executionPipeline), ICubeRenderFeature
	{
		private GraphicsPipelineDesc pPipelineDesc = new GraphicsPipelineDesc { Name = "Cube Pipeline" };

		private BufferDesc pVertexBufferDesc = new BufferDesc { 
			Name = "VBuffer",
			Usage = Usage.Immutable,
			BindFlags = BindFlags.VertexBuffer,
		};

		private BufferDesc pIndexBufferDesc = new BufferDesc { 
			Name = "IBuffer",
			Usage = Usage.Immutable,
			BindFlags = BindFlags.IndexBuffer
		};

		private IBuffer? pVertexBuffer;
		private IBuffer? pIndexBuffer;
		private IBuffer? pObjectCBuffer;
		private IBuffer? pCamCBuffer;
		private IPipelineState? pPipeline;


		private static readonly Vector3[] sVertices = new Vector3[]
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

		private static readonly uint[] sIndices = new uint[]
		{
			2,0,1, 2,3,0,
			4,6,5, 4,7,6,
			0,7,4, 0,3,7,
			1,0,4, 1,4,5,
			1,5,2, 5,6,2,
			3,6,7, 3,2,6
		};

		private static readonly Vector4[] sColors = new Vector4[]
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
		
		public Transform? Transform { get; set; }
		public Camera? Camera { get; set; }

		protected override void OnDispose()
		{
			pVertexBuffer?.Dispose();
			pIndexBuffer?.Dispose();

			pVertexBuffer = pIndexBuffer = null;
			pPipeline = null;

			base.OnDispose();
		}

		protected override async Task OnSetup(RenderFeatureSetupInfo setupInfo)
		{
			pVertexBuffer?.Dispose();
			pIndexBuffer?.Dispose();
			pPipeline?.Dispose();

			pVertexBuffer = CreateVertexBuffer(setupInfo.Driver.Device);
			pIndexBuffer = CreateIndexBuffer(setupInfo.Driver.Device);
			pPipeline = await CreatePipeline(
				setupInfo.PipelineStateManager, 
				setupInfo.ShaderManager, 
				setupInfo.BufferManager,
				setupInfo.AssetManager);
			pObjectCBuffer = setupInfo.BufferManager.GetBuffer(BufferGroupType.Object);
			pCamCBuffer = setupInfo.BufferManager.GetBuffer(BufferGroupType.Camera);
		}

		protected override void OnExecute(ICommandBuffer command)
		{
#if PROFILER
			using var _ = Profiler.Instance.Begin();
#endif
			var backbuffer = GetBackBuffer();
			var depthbuffer = GetDepthBuffer();

			if (Camera is null || Transform is null || backbuffer is null || depthbuffer is null)
				return;
			if (pVertexBuffer is null || pIndexBuffer is null || pObjectCBuffer is null || pPipeline is null)
				return;

			Camera.GetCBufferData(out var camCBufferData);

			UpdateCBuffer(command, pObjectCBuffer, Transform.WorldTransformMatrix);
			UpdateCBuffer(command, pCamCBuffer, camCBufferData);

			command
				.SetRT(backbuffer, depthbuffer)
				.SetVertexBuffers(0, new IBuffer[] { pVertexBuffer, pVertexBuffer }, new ulong[] { 0, (ulong)(Marshal.SizeOf<Vector3>() * sVertices.Length)})
				.SetIndexBuffer(pIndexBuffer)
				.SetPipeline(pPipeline)
				.CommitBindings(pPipeline.GetResourceBinding())
				.Draw(new DrawIndexedArgs
				{
					NumIndices = (uint)sIndices.Length
				});
		}

		private void UpdateCBuffer<T>(ICommandBuffer command, IBuffer buffer, in T data) where T : unmanaged
		{
			var mappedData = command.Map<T>(buffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = data;
			command.Unmap(buffer, MapType.Write);
		}

		protected virtual async Task<IShader> LoadShader(IShaderManager shaderMgr, ShaderType type, IAssetManager assetManager)
		{
			var shaderCI = new ShaderCreateInfo
			{
				Type = type
			};
			switch (type)
			{
				case ShaderType.Vertex:
					{
						shaderCI.Name = "Cube Vertex Shader";
						shaderCI.SourceCode = (await assetManager.GetAsyncAsset<ShaderAsset>("Shaders/cube_feat_vs.hlsl")).ShaderCode;
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Cube Pixel Shader";
						shaderCI.SourceCode = (await assetManager.GetAsyncAsset<ShaderAsset>("Shaders/cube_feat_ps.hlsl")).ShaderCode;
					}
					break;
				case ShaderType.Compute:
				case ShaderType.Geometry:
				case ShaderType.Hull:
				case ShaderType.Domain:
				default:
					throw new NotImplementedException();
			}
			
			return shaderMgr.GetOrCreate(shaderCI);
		}

		protected virtual async Task<IPipelineState> CreatePipeline(
			IPipelineStateManager pipelineMgr, 
			IShaderManager shaderMgr, 
			IBufferManager bufferMgr, 
			IAssetManager assetManager)
		{
			var vsShader = await LoadShader(shaderMgr, ShaderType.Vertex, assetManager);
			var psShader = await LoadShader(shaderMgr, ShaderType.Pixel, assetManager);

			pPipelineDesc.Output.RenderTargetFormats[0] = settings.DefaultColorFormat;
			pPipelineDesc.Output.DepthStencilFormat = settings.DefaultDepthFormat;
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
			var verticesSize = Marshal.SizeOf<Vector3>() * sVertices.Length;
			var colorsSize = Marshal.SizeOf<Vector4>() * sColors.Length;

			var buffer = new byte[verticesSize + colorsSize];
			pVertexBufferDesc.Size = (uint)buffer.Length;

			var handle = GCHandle.Alloc(sVertices, GCHandleType.Pinned);
			Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, verticesSize);
			handle.Free();

			handle = GCHandle.Alloc(sColors, GCHandleType.Pinned);
			Marshal.Copy(handle.AddrOfPinnedObject(), buffer, verticesSize, colorsSize);
			handle.Free();
			

			var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			var result = device.CreateBuffer(pVertexBufferDesc, bufferHandle.AddrOfPinnedObject(), (ulong)buffer.Length);
			bufferHandle.Free();

			return result;
		}
		protected virtual IBuffer CreateIndexBuffer(IDevice device)
		{
			pIndexBufferDesc.Size = (ulong)(Marshal.SizeOf<uint>() * sIndices.Length);
			return device.CreateBuffer(pIndexBufferDesc, sIndices);
		}
	}
}
