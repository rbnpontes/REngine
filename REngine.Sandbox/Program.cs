using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REngine.Sandbox
{
	internal static class Program
	{
		struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;

			public Vertex()
			{
				Position = new Vector3();
				UV = new Vector2();
			}

			public Vertex(Vector3 position, Vector2 uv)
			{
				Position = position;
				UV = uv;
			}
		}
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

			var cbuffer = LoadCBuffer(driver.Device);
			IPipelineState pipeline = CreatePSO(driver, swapChain);

			pipeline.GetResourceBinding().Set(ShaderTypeFlags.Vertex, "Constants", cbuffer);

			pipeline.GetResourceBinding().Set(ShaderTypeFlags.Pixel, "g_MainTexture", CreateTexture(driver.Device));

			(IBuffer vbuffer, IBuffer ibuffer) = LoadBuffers(driver.Device);

			var stopwatch = Stopwatch.StartNew();
			var cmd = driver.ImmediateCommand;

			form.Paint += (sender, e) =>
			{
				Matrix4x4 transform = GetTransform(stopwatch.ElapsedMilliseconds, swapChain.Size);
				var mappedData = cmd.Map<Matrix4x4>(cbuffer, MapType.Write, MapFlags.Discard);
				mappedData[0] = transform;
				cmd.Unmap(cbuffer, MapType.Write);

				cmd
					.SetRTs(new ITextureView[] { swapChain.ColorBuffer }, swapChain.DepthBuffer )
					.ClearRT(swapChain.ColorBuffer, Color.Black)
					.ClearDepth(swapChain.DepthBuffer, ClearDepthStencil.Depth, 1.0f, 0)
					.SetVertexBuffer(vbuffer)
					.SetIndexBuffer(ibuffer)
					.SetPipeline(pipeline)
					.CommitBindings(pipeline.GetResourceBinding())
					.Draw(new DrawIndexedArgs 
					{
						NumIndices = 36
					});

				swapChain.Present(false);
				form.Invalidate(new Rectangle(0, 0, 1, 1));
			};
			form.Resize += (sender, e) =>
			{
				var control = (Control)sender;
				swapChain.Size = new SwapChainSize((uint)control.Width, (uint)control.Height);
			};

			Application.Run(form);
			stopwatch.Stop();
		}

		private static IShader LoadShader(IDevice device, ShaderType type)
		{
			string suffix = string.Empty;
			string shaderName = string.Empty;

			switch (type) 
			{
				case ShaderType.Vertex:
					{
						suffix = "vs";
						shaderName = "Vertex Shader";
					}
					break;
				case ShaderType.Pixel:
					{
						suffix = "ps";
						shaderName = "Pixel Shader";
					}
					break;
			}

			string shaderFile = File.ReadAllText(
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cube_{suffix}.hlsl")
			);

			ShaderCreateInfo shaderCI = new ShaderCreateInfo
			{
				Name = shaderName,
				SourceCode = shaderFile,
				Type = type
			};

			return device.CreateShader(shaderCI);
		}
		private static (IBuffer, IBuffer) LoadBuffers(IDevice device)
		{
			Vertex[] vertices = new Vertex[]
			{
				new Vertex(new Vector3(-1, -1, -1), new Vector2(0, 1)),
				new Vertex(new Vector3(-1, +1, -1), new Vector2(0, 0)),
				new Vertex(new Vector3(+1, +1, -1), new Vector2(1, 0)),
				new Vertex(new Vector3(+1, -1, -1), new Vector2(1, 1)),

				new Vertex(new Vector3(-1, -1, +1), new Vector2(0, 1)),
				new Vertex(new Vector3(-1, +1, +1), new Vector2(0, 0)),
				new Vertex(new Vector3(+1, +1, +1), new Vector2(1, 0)),
				new Vertex(new Vector3(+1, -1, +1), new Vector2(1, 1))
			};
			uint[] indices = new uint[]
			{
				2,0,1, 2,3,0,
				4,6,5, 4,7,6,
				0,7,4, 0,3,7,
				1,0,4, 1,4,5,
				1,5,2, 5,6,2,
				3,6,7, 3,2,6
			};

			IBuffer vbuffer;
			IBuffer ibuffer;

			BufferDesc bufferDesc = new BufferDesc();
			bufferDesc.Name = "Cube VBuffer";
			bufferDesc.Usage = Usage.Immutable;
			bufferDesc.BindFlags = BindFlags.VertexBuffer;
			bufferDesc.Size = (ulong)(Unsafe.SizeOf<Vertex>() * vertices.Length);

			vbuffer = device.CreateBuffer(bufferDesc, vertices);

			bufferDesc.Name = "Cube IBuffer";
			bufferDesc.BindFlags = BindFlags.IndexBuffer;
			bufferDesc.Size = (ulong)(Unsafe.SizeOf<uint>() * indices.Length);

			ibuffer = device.CreateBuffer(bufferDesc, indices);

			return (vbuffer, ibuffer);
		}
		private static IBuffer LoadCBuffer(IDevice device)
		{
			return device.CreateBuffer(new BufferDesc
			{
				Name = "VS Cbuffer",
				Size = (ulong)Marshal.SizeOf<Matrix4x4>(),
				Usage = Usage.Dynamic,
				BindFlags = BindFlags.UniformBuffer,
				AccessFlags = CpuAccessFlags.Write
			});
		}

		private static IPipelineState CreatePSO(IGraphicsDriver driver, ISwapChain swapChain)
		{
			IShader vsShader = LoadShader(driver.Device, ShaderType.Vertex);
			IShader psShader = LoadShader(driver.Device, ShaderType.Pixel);

			GraphicsPipelineDesc desc = new GraphicsPipelineDesc();
			desc.Name = "Cube PSO";
			desc.Output.RenderTargetFormats[0] = swapChain.Desc.Formats.Color;
			desc.Output.DepthStencilFormat = swapChain.Desc.Formats.Depth;
			desc.BlendState.BlendMode = BlendMode.Replace;
			desc.PrimitiveType = PrimitiveType.TriangleList;
			desc.RasterizerState.CullMode = CullMode.Back;
			desc.DepthStencilState.EnableDepth = true;

			desc.Shaders.VertexShader = vsShader;
			desc.Shaders.PixelShader = psShader;

			desc.InputLayouts = new PipelineInputLayoutElementDesc[]
			{
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 0,
					Input = new InputLayoutElementDesc
					{
						ElementType = ElementType.Vector3,
					}
				},
				new PipelineInputLayoutElementDesc
				{
					InputIndex = 1,
					Input = new InputLayoutElementDesc
					{
						ElementType = ElementType.Vector2
					}
				}
			};

			var pipeline = driver.Device.CreateGraphicsPipeline(desc);

			vsShader.Dispose();
			psShader.Dispose();

			return pipeline;
		}
	
		private static ITextureView CreateTexture(IDevice device)
		{
			var bitmap = new Bitmap(
				Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg")
			);
			var bitmapData = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb
			);

			var pixelData = new byte[bitmapData.Stride * bitmap.Height];
			Marshal.Copy(bitmapData.Scan0, pixelData, 0, bitmapData.Stride * bitmapData.Height);
			bitmap.UnlockBits(bitmapData);

			for (var pixelIdx = 0; pixelIdx < bitmap.Width * bitmap.Height; pixelIdx++)
				(pixelData[4 * pixelIdx + 0], pixelData[4 * pixelIdx + 2]) = (pixelData[4 * pixelIdx + 2], pixelData[4 * pixelIdx + 0]);

			return device.CreateTexture(new TextureDesc
			{
				Name = "Doge Texture",
				Size = new TextureSize((uint)bitmap.Width, (uint)bitmap.Height),
				Format = TextureFormat.RGBA8UNormSRGB,
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable
			}, new ITextureData[] { 
				new ByteTextureData(pixelData, (ulong)bitmapData.Stride) 
			}).GetDefaultView(TextureViewType.ShaderResource);
		}

		private static Matrix4x4 GetTransform(long elapsedMilleseconds, SwapChainSize size)
		{
			var worldMatrix = Matrix4x4.CreateRotationY(elapsedMilleseconds / 1000.0f) * Matrix4x4.CreateRotationX(-MathF.PI * 0.1f);
			var viewMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 5.0f);
			var projMatrix = CreatePerspectiveFieldOfView((float)Math.PI / 4.0f, size.Width / (float)size.Height, 0.01f, 100.0f, false);
			return Matrix4x4.Transpose(worldMatrix * viewMatrix * projMatrix);
		}

		private static Matrix4x4 CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isOpenGL)
		{
			if (fieldOfView <= 0.0f || fieldOfView >= MathF.PI)
				throw new ArgumentOutOfRangeException(nameof(fieldOfView));

			if (nearPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

			if (farPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

			if (nearPlaneDistance >= farPlaneDistance)
				throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

			float yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
			float xScale = yScale / aspectRatio;

			Matrix4x4 result = new()
			{
				M11 = xScale,
				M22 = yScale
			};

			if (isOpenGL)
			{
				result.M33 = (farPlaneDistance + nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
				result.M43 = -2 * nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M34 = 1.0f;
			}
			else
			{
				result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M43 = -nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M34 = 1.0f;
			}

			return result;
		}

		private static void HandleMessage(object sender, MessageEventArgs args)
		{
			switch (args.Severity)
			{
				case DbgMsgSeverity.Warning:
				case DbgMsgSeverity.Error:
				case DbgMsgSeverity.FatalError:
					Debug.WriteLine($"Diligent Engine: {args.Severity} in {args.Function}() ({args.File}, {args.Line}): {args.Message}");
					break;
				case DbgMsgSeverity.Info:
					Debug.WriteLine($"Diligent Engine: {args.Severity} {args.Message}");
					break;
			}
		}
	}
}