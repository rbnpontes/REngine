using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	public interface ICubeRenderFeature : IRenderFeature 
	{ 
		public Matrix4x4 Transform { get; set; }
	}

	internal class CubeRenderFeature : ICubeRenderFeature
	{
		protected GraphicsSettings pSettings;
		protected GraphicsPipelineDesc pPipelineDesc = new GraphicsPipelineDesc { Name = "Cube PSO " };
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
		protected IBuffer? pCBuffer;
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

		public bool IsDirty { get; private set; } = true;
		public bool IsDisposed { get; private set; } = false;

		public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

		public CubeRenderFeature(GraphicsSettings settings)
		{
			pSettings = settings;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;

			pVertexBuffer?.Dispose();
			pIndexBuffer?.Dispose();
			pPipeline?.Dispose();

			pVertexBuffer = pIndexBuffer = null;
			pPipeline = null;
		}

		public IRenderFeature MarkAsDirty()
		{
			IsDirty = true;
			return this;
		}

		public IRenderFeature Setup(IGraphicsDriver driver, IRenderer renderer)
		{
			if (IsDisposed)
				return this;

			Dispose();
			IsDisposed = false;

			pVertexBuffer = CreateVertexBuffer(driver.Device);
			pIndexBuffer = CreateIndexBuffer(driver.Device);
			pPipeline = CreatePipeline(driver.Device, renderer);
			pSwapChain = renderer.SwapChain;
			pCBuffer = renderer.GetBuffer(BufferGroupType.Object);

			IsDirty = false;
			return this;
		}

		public IRenderFeature Compile(ICommandBuffer command)
		{
			return this;
		}

		public IRenderFeature Execute(ICommandBuffer command)
		{ 
			if (pVertexBuffer is null || pIndexBuffer is null || pCBuffer is null || pPipeline is null || pSwapChain is null)
				return this;

			var mappedData = command.Map<Matrix4x4>(pCBuffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = Transform;
			command.Unmap(pCBuffer, MapType.Write);

			command
				.SetRTs(new ITextureView[] { pSwapChain.ColorBuffer }, pSwapChain.DepthBuffer)
				.SetVertexBuffer(pVertexBuffer)
				.SetIndexBuffer(pIndexBuffer)
				.SetPipeline(pPipeline)
				.CommitBindings(pPipeline.GetResourceBinding())
				.Draw(new DrawIndexedArgs
				{
					NumIndices = (uint)pIndices.Length
				});
			return this;
		}

		protected virtual IShader LoadShader(IDevice device, ShaderType type)
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
						shaderPath = Path.Join(shaderPath, "cube_feat_vs.hlsl");
					}
					break;
				case ShaderType.Pixel:
					{
						shaderCI.Name = "Cube Pixel Shader";
						shaderPath = Path.Join(shaderPath, "cube_feat_ps.hlsl");
					}
					break;
				default:
					throw new NotImplementedException();
			}

			shaderCI.SourceCode = File.ReadAllText(shaderPath);

			return device.CreateShader(shaderCI);
		}

		protected virtual IPipelineState CreatePipeline(IDevice device, IRenderer renderer)
		{
			IShader vsShader = LoadShader(device, ShaderType.Vertex);
			IShader psShader = LoadShader(device, ShaderType.Pixel);

			pPipelineDesc.Output.RenderTargetFormats[0] = pSettings.DefaultColorFormat;
			pPipelineDesc.Output.DepthStencilFormat = pSettings.DefaultDepthFormat;
			pPipelineDesc.BlendState.BlendMode = BlendMode.Replace;
			pPipelineDesc.PrimitiveType = PrimitiveType.TriangleList;
			pPipelineDesc.RasterizerState.CullMode = CullMode.Back;
			pPipelineDesc.DepthStencilState.EnableDepth = true;

			pPipelineDesc.Shaders.VertexShader = vsShader;
			pPipelineDesc.Shaders.PixelShader = psShader;

			SetupInputLayout(pPipelineDesc.InputLayouts);

			var pipeline = device.CreateGraphicsPipeline(pPipelineDesc);

			pipeline.GetResourceBinding().Set(ShaderTypeFlags.Vertex, "Constants", renderer.GetBuffer(BufferGroupType.Object));

			vsShader.Dispose();
			psShader.Dispose();

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
						BufferStride = (uint)(Marshal.SizeOf<Vector3>()),
						ElementOffset = 0,
						ElementType = ElementType.Vector3
					}
				}
			);
			//inputLayout.Add(new PipelineInputLayoutElementDesc
			//{
			//	InputIndex = 1,
			//	Input = new InputLayoutElementDesc
			//	{
			//		ElementType = ElementType.Vector4,
			//		ElementOffset = (uint)(Marshal.SizeOf<Vector4>()),
			//		BufferStride = (uint)(Marshal.SizeOf<Vector3>() * pVertices.Length)
			//	}
			//});
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
