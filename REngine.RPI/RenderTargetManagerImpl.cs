using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.RHI;

namespace REngine.RPI
{
	internal class RenderTargetManagerImpl : IRenderTargetManager, IDisposable
	{
		private class TextureWrapper : ITexture
		{
			private readonly RenderTargetManagerImpl pRtMgr;
			private readonly ITexture pTarget;

			public LinkedListNode<TextureWrapper>? Node;
			public IntPtr Handle => pTarget.Handle;
			public bool IsDisposed { get; private set; }

			public event EventHandler? OnDispose;
			public GPUObjectType ObjectType => pTarget.ObjectType;

			public string Name => pTarget.Name;

			public TextureDesc Desc => pTarget.Desc;

			public ResourceState State
			{
				get => pTarget.State;
				set => pTarget.State = value;
			}

			public ulong GPUHandle => pTarget.GPUHandle;

			public TextureWrapper(ITexture texture, RenderTargetManagerImpl rtMgr)
			{
				pTarget = texture;
				pRtMgr = rtMgr;
			}
			public void Dispose()
			{
				if (IsDisposed)
					return;

				var desc = Desc;
				if(Node != null)
					pRtMgr.pRenderTargets.Remove(Node);
				pTarget.Dispose();

				pRtMgr.pLogger.Info($"Disposed Render Target({desc.Size.Width}x{desc.Size.Height}:{desc.Format})");
				IsDisposed = true;
			}

			public ITextureView GetDefaultView(TextureViewType view)
			{
				return pTarget.GetDefaultView(view);
			}
		}


		private readonly IServiceProvider pServiceProvider;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly EngineEvents pEngineEvents;
		private readonly ILogger<IRenderTargetManager> pLogger;
		private readonly LinkedList<TextureWrapper> pRenderTargets = new();

		private bool pDisposed;
		private IDevice? pDevice;
		private ITexture? pDummyTexture;

		public RenderTargetManagerImpl(
			IServiceProvider provider, 
			GraphicsSettings graphicsSettings,
			EngineEvents engineEvents,
			ILoggerFactory loggerFactory
		)
		{
			pServiceProvider = provider;
			pGraphicsSettings = graphicsSettings;
			pEngineEvents = engineEvents;
			pLogger = loggerFactory.Build<IRenderTargetManager>();

			engineEvents.OnStart.Once(HandleEngineStart);
			engineEvents.OnStop.Once(HandleEngineStop);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			var nextNode = pRenderTargets.First;
			while (nextNode is not null)
			{
				nextNode.Value.Node = null;
				nextNode.Value.Dispose();

				nextNode = nextNode.Next;
			}

			pDisposed = true;
		}

		private async Task HandleEngineStop(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			Dispose();
		}

		private async Task HandleEngineStart(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			pLogger.Debug("Initializing");
			pDevice = pServiceProvider.Get<IGraphicsDriver>().Device;
			pDummyTexture = AllocateDummyTexture();
			pLogger.Debug("Initialized");
		}

		public ITexture GetDummyTexture()
		{
			pDummyTexture ??= AllocateDummyTexture();
			return pDummyTexture;
		}

		public ITexture Allocate(uint width, uint height)
		{
			return Allocate(width, height, pGraphicsSettings.DefaultColorFormat);
		}

		public ITexture Allocate(uint width, uint height, TextureFormat format)
		{
			GetDesc(width, height, format, out var desc);
			var texture = GetDevice().CreateTexture(desc);
			var wrapper = new TextureWrapper(texture, this);
			var node = pRenderTargets.AddLast(wrapper);
			wrapper.Node = node;

			pLogger.Info($"Allocated Render Target({width}x{height}:{format})");
			return wrapper;
		}

		public ITexture AllocateDepth(uint width, uint height)
		{
			return Allocate(width, height, pGraphicsSettings.DefaultColorFormat);
		}
		public ITexture AllocateDepth(uint width, uint height, TextureFormat format)
		{
			GetDesc(width, height, format, out var desc);
			desc.BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil;
			var texture = GetDevice().CreateTexture(desc);
			var wrapper = new TextureWrapper(texture, this);
			var node = pRenderTargets.AddLast(wrapper);
			wrapper.Node = node;

			pLogger.Info($"Allocated Depth Buffer({width}x{height}:{format})");
			return wrapper;
		}
		private ITexture AllocateDummyTexture()
		{
			GetDesc(1, 1, pGraphicsSettings.DefaultColorFormat, out var desc);
			desc.Name = "Dummy Texture";
			return GetDevice().CreateTexture(desc);
		}

		private void GetDesc(uint width, uint height, TextureFormat format, out TextureDesc output)
		{
			output = new()
			{
				Name = $"Render Target - ({width}x{height}:{format})",
				Size = new TextureSize(width, height),
				Format = format,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				AccessFlags = CpuAccessFlags.None,
				MipLevels = 1,
				Dimension = TextureDimension.Tex2D,
				ClearValue = new TextureClearValue()
				{
					A = 1,
					R = 0,
					G = 0,
					B = 0,
					Format = pGraphicsSettings.DefaultColorFormat,
				}
			};
		}
		private IDevice GetDevice()
		{
			return pDevice ??
			       throw new NullReferenceException("Device is null. Did you get device before engine start ?");
		}
	}
}
