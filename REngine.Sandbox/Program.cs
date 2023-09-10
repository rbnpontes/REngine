using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RHI.DiligentDriver;

namespace REngine.Sandbox
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();

			var serviceProvider = new LazyServiceProvider();
			serviceProvider.AddService(new GraphicsSettings());

			var form = new Form
			{
				Name = "REngine",
				Text = "REngine",
				FormBorderStyle = FormBorderStyle.Sizable,
				ClientSize = new Size(500, 500),
				StartPosition = FormStartPosition.CenterScreen
			};

			var factory = new GraphicsFactory(serviceProvider);
			factory.OnMessage += HandleMessage;

			ISwapChain swapChain;
			IGraphicsDriver driver = factory.Create(new GraphicsFactoryCreateInfo
			{
				Settings = new GraphicsDriverSettings
				{
					EnableValidation = true,
					Backend = GraphicsBackend.D3D11
				},
				WindowHandle = form.Handle
			}, new SwapChainDesc { Size = new SwapChainSize((uint)form.Width, (uint)form.Height) }, out swapChain);

			IPipelineState pipeline = CreatePSO(driver, swapChain);

			var cmd = driver.ImmediateCommand;

			form.Paint += (sender, e) =>
			{
				cmd
					.SetRTs(new ITextureView[] { swapChain.ColorBuffer }, swapChain.DepthBuffer )
					.ClearRT(swapChain.ColorBuffer, Color.Black)
					.ClearDepth(swapChain.DepthBuffer, ClearDepthStencil.Depth, 1.0f, 0)
					.SetPipeline(pipeline)
					.Draw(new DrawArgs { NumVertices = 3 });

				swapChain.Present(false);
				form.Invalidate(new Rectangle(0, 0, 1, 1));
			};
			form.Resize += (sender, e) =>
			{
				var control = (Control)sender;
				swapChain.Size = new SwapChainSize((uint)control.Width, (uint)control.Height);
			};

			Application.Run(form);
		}

		private static IPipelineState CreatePSO(IGraphicsDriver driver, ISwapChain swapChain)
		{
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
				vsShader = driver.Device.CreateShader(shaderCI);
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
				psShader = driver.Device.CreateShader(shaderCI);
			}

			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Triangle PSO";
			desc.Output.RenderTargetFormats[0] = swapChain.Desc.Formats.Color;
			desc.Output.DepthStencilFormat = swapChain.Desc.Formats.Depth;
			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Both;
			desc.DepthStencilState.EnableDepth = false;

			desc.Shaders.VertexShader = vsShader;
			desc.Shaders.PixelShader = psShader;

			var pipeline = driver.Device.CreateGraphicsPipeline(desc);

			vsShader.Dispose();
			psShader.Dispose();

			return pipeline;
		}
	
		private static void HandleMessage(object sender, MessageEventArgs args)
		{
			switch (args.Severity)
			{
				case DbgMsgSeverity.Warning:
				case DbgMsgSeverity.Error:
				case DbgMsgSeverity.FatalError:
					Console.WriteLine($"Diligent Engine: {args.Severity} in {args.Function}() ({args.File}, {args.Line}): {args.Message}");
					break;
				case DbgMsgSeverity.Info:
					Console.WriteLine($"Diligent Engine: {args.Severity} {args.Message}");
					break;
			}
		}
	}
}