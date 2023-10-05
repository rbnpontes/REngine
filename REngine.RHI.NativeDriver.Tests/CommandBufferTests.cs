using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.Tests
{
	[TestFixture, Apartment(ApartmentState.STA)]
	public class CommandBufferTests : BaseTest
	{
		const int MaxRenderTimeMs = 100;
		private ISwapChain? pSwapChain;
		private IGraphicsDriver? pDriver;

		[SetUp]
		public void SetupResources()
		{
			CreateWindow();
			(IGraphicsDriver driver, ISwapChain swapChain) = CreateDriver(GraphicsBackend.D3D11);
			pDriver = driver;
			pSwapChain = swapChain;
		}

		[TearDown]
		public void CleanupResources()
		{
			CleanDisposables();
		}

		[Test]
		public void MustRenderTriangle()
		{
			var depthBuffer = pSwapChain?.DepthBuffer;
			if (pDriver == null)
				throw new NullReferenceException();
			if (pSwapChain == null)
				throw new NullReferenceException();
			if (depthBuffer == null)
				throw new NullReferenceException();
			if (MainWindow == null)
				throw new NullReferenceException();

			IShader vsShader;
			IShader psShader;

			ShaderCreateInfo shaderCI;
			{
				shaderCI = new ShaderCreateInfo
				{
					Name = "VS Shader",
					SourceCode =
						"struct PSInput" +
						"{" +
						"	float4 pos : SV_POSITION;" +
						"	float3 color : COLOR;" +
						"};" +
						"" +
						"void main(in uint vertId : SV_VertexID, out PSInput input)" +
						"{" +
						"	float4 pos[3];" +
						"	pos[0] = float4(-0.5, -0.5, 0.0, 1.0);" +
						"	pos[1] = float4( 0.0, +0.5, 0.0, 1.0);" +
						"	pos[2] = float4(+0.5, -0.5, 0.0, 1.0);" +
						"" +
						"	float3 col[3];" +
						"	col[0] = float3(1.0, 0.0, 0.0);" +
						"	col[1] = float3(0.0, 1.0, 0.0);" +
						"	col[2] = float3(0.0, 0.0, 1.0);" +
						"" +
						"	input.pos = pos[vertId];" +
						"	input.color = col[vertId];" +
						"}"
					,
					Type = ShaderType.Vertex
				};
				vsShader = pDriver.Device.CreateShader(shaderCI);
				Disposables.Add(vsShader);
			}
			{
				shaderCI = new ShaderCreateInfo
				{
					Name = "PS Shader",
					SourceCode =
						"struct PSInput" +
						"{" +
						"	float4 pos : SV_POSITION;" +
						"	float3 color : COLOR;" +
						"};" +
						"" +
						"struct PSOutput" +
						"{" +
						"	float4 color : SV_TARGET;" +
						"};" +
						"" +
						"void main(in PSInput input, out PSOutput output)" +
						"{" +
						"	output.color = float4(input.color.rgb, 1.0);" +
						"}",
					Type = ShaderType.Pixel
				};
				psShader = pDriver.Device.CreateShader(shaderCI);
				Disposables.Add(psShader);
			}

			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Triangle PSO";
			desc.Output.RenderTargetFormats[0] = pSwapChain.Desc.Formats.Color;
			desc.Output.DepthStencilFormat = pSwapChain.Desc.Formats.Depth;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = false;

			desc.Shaders.VertexShader = vsShader;
			desc.Shaders.PixelShader = psShader;

			var pipeline = pDriver.Device.CreateGraphicsPipeline(desc);
			Disposables.Add(pipeline);

			var cmd = pDriver.ImmediateCommand;

			var stopwatch = Stopwatch.StartNew();
			MainWindow.Paint += (sender, e) =>
			{
				cmd
					.SetRTs(new ITextureView[] { pSwapChain.ColorBuffer }, depthBuffer)
					.ClearRT(pSwapChain.ColorBuffer, Color.Gray)
					.ClearDepth(depthBuffer, ClearDepthStencil.Depth, 1.0f, 0)
					.SetPipeline(pipeline)
					.Draw(new DrawArgs { NumVertices = 3 });
				pSwapChain.Present(false);
				MainWindow.Invalidate(new Rectangle(0, 0, 1, 1));
				// stop draw at 10 secs
				if (stopwatch.ElapsedMilliseconds >= MaxRenderTimeMs)
					MainWindow.Close();
			};
			MainWindow.Resize += (sender, e) =>
			{
				var size = MainWindow.Size;
				pSwapChain.Size = new SwapChainSize((uint)size.Width, (uint)size.Height);
			};

			Application.Run(MainWindow);
			Assert.Pass();
		}
	}
}
