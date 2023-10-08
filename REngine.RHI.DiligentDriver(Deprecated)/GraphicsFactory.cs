using Diligent;
using REngine.RHI.DiligentDriver.Adapters;

namespace REngine.RHI.DiligentDriver
{
	public class GraphicsFactoryCreateInfo
	{
		public GraphicsDriverSettings Settings { get; set; } = new GraphicsDriverSettings();
		/// <summary>
		/// Required on GraphicsBackend.OpenGL
		/// </summary>
		public IntPtr WindowHandle { get; set; } = IntPtr.Zero;
	}

	public class GraphicsFactory
	{
		public event MessageEvent OnMessage = new MessageEvent((obj, e) => { });

		private IServiceProvider pProvider;
		public GraphicsFactory(IServiceProvider provider)
		{
			pProvider = provider;
		}

		/// <summary>
		/// Validate CreateInfo
		/// </summary>
		/// <param name="createInfo"></param>
		public void Validate(GraphicsFactoryCreateInfo createInfo)
		{
			if (createInfo.Settings.Backend == GraphicsBackend.OpenGL && createInfo.WindowHandle == IntPtr.Zero)
				throw new ArgumentException("createInfo.WindowHandle is zero. OpenGL requires WindowHandle to create SwapChain.");
		}
		
		public GraphicsAdapter[] GetAvailableAdapters(GraphicsBackend backend)
		{
			Diligent.Version graphicsVersion = new Diligent.Version();
			
			if (backend == GraphicsBackend.D3D11 || backend == GraphicsBackend.D3D12)
				graphicsVersion = new Diligent.Version(11, 0);
			//else if (backend == GraphicsBackend.D3D12)
			//	graphicsVersion = new Diligent.Version(12, 0);

			using IEngineFactory factory = backend switch
			{
				GraphicsBackend.D3D11 => Native.GetEngineFactoryD3D11(),
				GraphicsBackend.D3D12 => Native.GetEngineFactoryD3D12(),
				GraphicsBackend.Vulkan => Native.GetEngineFactoryVk(),
				GraphicsBackend.OpenGL => Native.GetEngineFactoryOpenGL(),
				_ => throw new NotSupportedException($"Not supported this backend type {backend}.")
			};

			if (backend == GraphicsBackend.D3D12)
				(factory as IEngineFactoryD3D12)?.LoadD3D12("d3d12.dll");

			SetupMessageEvent(factory);

			var adapters = factory.EnumerateAdapters(graphicsVersion);
			factory.Dispose();

			return adapters.Select(adapter =>
			{
				return new GraphicsAdapter
				{
					Id = adapter.DeviceId,
					VendorId = adapter.VendorId,
					Name = adapter.Description,
					AdapterType = (AdapterType)adapter.Type,
				};
			}).ToArray();
		}
		
		public IGraphicsDriver Create(GraphicsFactoryCreateInfo createInfo)
		{
			if (createInfo.Settings.Backend == GraphicsBackend.OpenGL)
				throw new NotSupportedException("This call does not support OpenGL backend, you must call Create(createInfo, out swapChain) instead.");
			(IGraphicsDriver driver, Diligent.ISwapChain _) = CreateWithSwapChain(createInfo, null);
			return driver;
		}
		
		public IGraphicsDriver Create(GraphicsFactoryCreateInfo createInfo, RHI.SwapChainDesc swapChainDesc, out RHI.ISwapChain swapChain)
		{
			Validate(createInfo);
			(IGraphicsDriver driver, Diligent.ISwapChain? nativeSwapChain) = CreateWithSwapChain(createInfo, swapChainDesc);
			if (nativeSwapChain is null)
				throw new NullReferenceException("SwapChain is null");
			swapChain = new SwapChainImpl(nativeSwapChain);
			return driver;
		}

		private (IGraphicsDriver, Diligent.ISwapChain?) CreateWithSwapChain(GraphicsFactoryCreateInfo createInfo, RHI.SwapChainDesc? swapChainDesc)
		{
			GraphicsDriverImpl impl = new GraphicsDriverImpl(pProvider);
			impl.Backend = createInfo.Settings.Backend;

			Diligent.SwapChainDesc nativeSwapChainDesc = new Diligent.SwapChainDesc();
			Diligent.ISwapChain? swapChain = null;
			IRenderDevice renderDevice;
			IDeviceContext[] deviceContexts;

			if (swapChainDesc.HasValue)
			{
				var desc = swapChainDesc.Value;
				var adapter = new SwapChainAdapter();
				adapter.Fill(ref desc, out nativeSwapChainDesc);
			}

			uint deferredCtxCount = Math.Max((uint)Environment.ProcessorCount, 2);

			switch (createInfo.Settings.Backend)
			{
#if WINDOWS
				case GraphicsBackend.D3D11:
					{
						IEngineFactoryD3D11 engineFactory = Native.GetEngineFactoryD3D11();
						SetupMessageEvent(engineFactory);

						engineFactory.CreateDeviceAndContextsD3D11(new EngineD3D11CreateInfo
						{
							EnableValidation = createInfo.Settings.EnableValidation,
							AdapterId = createInfo.Settings.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;

						if (swapChainDesc != null)
							swapChain = engineFactory.CreateSwapChainD3D11(
								renderDevice,
								deviceContexts[0],
								in nativeSwapChainDesc,
								new FullScreenModeDesc { },
								new Win32NativeWindow { Wnd = createInfo.WindowHandle });
					}
					break;
				case GraphicsBackend.D3D12:
					{
						IEngineFactoryD3D12 engineFactory = Native.GetEngineFactoryD3D12();
						SetupMessageEvent(engineFactory);

						engineFactory.LoadD3D12("d3d12.dll");

						engineFactory.CreateDeviceAndContextsD3D12(new EngineD3D12CreateInfo
						{
							EnableValidation = createInfo.Settings.EnableValidation,
							AdapterId = createInfo.Settings.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;

						if (swapChainDesc != null)
							swapChain = engineFactory.CreateSwapChainD3D12(
								renderDevice,
								deviceContexts[0],
								in nativeSwapChainDesc,
								new FullScreenModeDesc { },
								new Win32NativeWindow { Wnd = createInfo.WindowHandle }
								);
					}
					break;
#endif
				case GraphicsBackend.Vulkan:
					{
						IEngineFactoryVk engineFactory = Native.GetEngineFactoryVk();
						SetupMessageEvent(engineFactory);

						engineFactory.CreateDeviceAndContextsVk(new EngineVkCreateInfo
						{
							EnableValidation = createInfo.Settings.EnableValidation,
							AdapterId = createInfo.Settings.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;

						if (swapChainDesc != null)
							swapChain = engineFactory.CreateSwapChainVk(
								renderDevice,
								deviceContexts[0],
								in nativeSwapChainDesc,
								new Win32NativeWindow { Wnd = createInfo.WindowHandle });
					}
					break;
				case GraphicsBackend.OpenGL:
					{
						if (swapChainDesc is null)
							throw new ArgumentNullException("SwapChain Desc is required on OpenGL backend");
						IDeviceContext immediateCtx;
						IEngineFactoryOpenGL engineFactory = Native.GetEngineFactoryOpenGL();

						SetupMessageEvent(engineFactory);

						engineFactory.CreateDeviceAndSwapChainGL(new EngineGLCreateInfo
						{
							AdapterId = createInfo.Settings.AdapterId,
							EnableValidation = createInfo.Settings.EnableValidation,
							Window = new Win32NativeWindow { Wnd = createInfo.WindowHandle },
						}, out renderDevice, out immediateCtx, in nativeSwapChainDesc, out swapChain);
						deviceContexts = new IDeviceContext[] { immediateCtx };
						impl.EngineFactory = engineFactory;
					}
					break;
				default:
					throw new NotSupportedException($"Not supported this backend type {createInfo.Settings.Backend}");
			}

			impl.Device = new DeviceImpl(impl, renderDevice);

			bool isImmediate = true;
			impl.Commands = deviceContexts.Select(x => {
				var cmd = new CommandBufferImpl(x, !isImmediate);
				if (isImmediate)
					isImmediate = false;
				return cmd;
			}).ToList().AsReadOnly();

			return (impl, swapChain);
		}
		
		private void SetupMessageEvent(IEngineFactory engineFactory)
		{
			engineFactory.SetMessageCallback((severity, msg, func, file, line) =>
			{
				OnMessage.Invoke(engineFactory, new MessageEventArgs
				{
					File = file,
					Line = line,
					Function = func,
					Message = msg,
					Severity = (DbgMsgSeverity)severity
				});
			});
		}
	}
}
