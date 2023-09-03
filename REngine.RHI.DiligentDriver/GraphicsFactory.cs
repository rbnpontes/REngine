using Diligent;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diligent.IEngineFactory;

namespace REngine.RHI.DiligentDriver
{
	public class GraphicsFactoryCreateInfo
	{
#if DEBUG
		public bool EnableValidation { get; set; } = true;
# else
		public bool EnableValidation { get; set; } = false;
# endif
		public uint AdapterId { get; set; }
		public GraphicsBackend Backend { get; set; }
		/// <summary>
		/// Required on GraphicsBackend.OpenGL
		/// </summary>
		public IntPtr WindowHandle { get; set; } = IntPtr.Zero;
		public IDictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();
		public MessageCallbackDelegate? MessageCallback { get; set; }
	}

	public static class GraphicsFactory
	{
		/// <summary>
		/// Validate CreateInfo
		/// </summary>
		/// <param name="createInfo"></param>
		public static void Validate(GraphicsFactoryCreateInfo createInfo)
		{
			if (createInfo.Backend == GraphicsBackend.OpenGL && createInfo.WindowHandle == IntPtr.Zero)
				throw new ArgumentException("createInfo.WindowHandle is zero. OpenGL requires WindowHandle to create SwapChain.");
		}
		public static IGraphicsDriver Create(GraphicsFactoryCreateInfo createInfo)
		{
			if (createInfo.Backend == GraphicsBackend.OpenGL)
				throw new NotSupportedException("This call does not support OpenGL backend, you must call Create(createInfo, out swapChain) instead.");
			(IGraphicsDriver driver, ISwapChain _) = CreateWithSwapChain(createInfo, new SwapChainDesc { });
			return driver;
		}

		private static (IGraphicsDriver, ISwapChain?) CreateWithSwapChain(GraphicsFactoryCreateInfo createInfo, SwapChainDesc swapChainDesc)
		{
			GraphicsDriverImpl impl = new GraphicsDriverImpl();
			impl.Backend = createInfo.Backend;

			ISwapChain? swapChain = null;
			IRenderDevice renderDevice;
			IDeviceContext[] deviceContexts;

			uint deferredCtxCount = Math.Max((uint)Environment.ProcessorCount, 2);

			switch (createInfo.Backend)
			{
#if WINDOWS
				case GraphicsBackend.D3D11:
					{
						IEngineFactoryD3D11 engineFactory = Native.GetEngineFactoryD3D11();
						engineFactory.CreateDeviceAndContextsD3D11(new EngineD3D11CreateInfo
						{
							EnableValidation = createInfo.EnableValidation,
							AdapterId = createInfo.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;
					}
					break;
				case GraphicsBackend.D3D12:
					{
						IEngineFactoryD3D12 engineFactory = Native.GetEngineFactoryD3D12();
						engineFactory.CreateDeviceAndContextsD3D12(new EngineD3D12CreateInfo
						{
							EnableValidation = createInfo.EnableValidation,
							AdapterId = createInfo.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;
					}
					break;
#endif
				case GraphicsBackend.Vulkan:
					{
						IEngineFactoryVk engineFactory = Native.GetEngineFactoryVk();
						engineFactory.CreateDeviceAndContextsVk(new EngineVkCreateInfo
						{
							EnableValidation = createInfo.EnableValidation,
							AdapterId = createInfo.AdapterId,
							NumDeferredContexts = deferredCtxCount,
						}, out renderDevice, out deviceContexts);
						impl.EngineFactory = engineFactory;
					}
					break;
				case GraphicsBackend.OpenGL:
					{
						IDeviceContext immediateCtx;
						IEngineFactoryOpenGL engineFactory = Native.GetEngineFactoryOpenGL();
						engineFactory.CreateDeviceAndSwapChainGL(new EngineGLCreateInfo
						{
							AdapterId = createInfo.AdapterId,
							EnableValidation = createInfo.EnableValidation,
							NumDeferredContexts = deferredCtxCount,
							Window = new Win32NativeWindow { Wnd = createInfo.WindowHandle },
						}, out renderDevice, out immediateCtx, in swapChainDesc, out swapChain);
						deviceContexts = new IDeviceContext[] { immediateCtx };
						impl.EngineFactory = engineFactory;
					}
					break;
				default:
					throw new NotSupportedException($"Not supported this backend type {createInfo.Backend}");
			}

			impl.Device = new DeviceImpl(renderDevice);
			impl.Commands = deviceContexts.Select(x => new CommandBufferImpl(x)).ToList().AsReadOnly();

			return (impl, swapChain);
		}
	}
}
