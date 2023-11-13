using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Exceptions;
using REngine.Core.IO;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class RendererImpl : IRenderer
	{
		[Flags]
		public enum BufferUpdateFlags
		{
			None = 0,
			Fixed = 1 << 0,
			Frame = 1 << 1,
			Object = 1 << 2,
			All = Fixed | Frame | Object
		}

		class RenderFeatureEntry
		{
			public int ZIndex { get; set; }
			public IRenderFeature Feature { get; set; }

			public RenderFeatureEntry(IRenderFeature feature)
			{
				Feature = feature;
			}
		}

		private readonly IServiceProvider pProvider;
		private readonly ILogger<IRenderer> pLogger;
		private readonly RPIEvents pRenderEvents;
		private readonly RenderState pRenderState;
		private readonly IBufferManager pBufferProvider;
		private readonly IPipelineStateManager pPipelineMgr;
		private readonly IShaderManager pShaderMgr;
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly IEngine pEngine;

#if RENGINE_RENDERGRAPH
		private RenderGraph.IResourceManager? pRenderGraphResMgr;
		private RenderGraph.IResource? pMainBackbufferResource;
		private RenderGraph.IResource? pMainDepthbufferResource;
#endif
		private IExecutionPipelineVar pNeedsPrepareVar;

		private readonly FeatureCollection pFeatureCollection = new FeatureCollection();

		private IGraphicsDriver? pDriver;
		private bool pDisposed = false;

		private ISwapChain? pSwapChain;

		public bool IsDisposed { get => pDisposed; }

		public ISwapChain? SwapChain { 
			get => pSwapChain; 
			set => UpdateSwapChain(value); 
		}
		public IGraphicsDriver? Driver
		{
			get => pDriver;
			set
			{
				if (pDriver != null)
					return;
				pDriver = value;
			}
		}

		public RendererImpl(
			IServiceProvider provider, 
			ILogger<IRenderer> logger,
			EngineEvents events,
			RPIEvents rendererEvts,
			RenderState renderState,
			IBufferManager bufferProvider,
			IPipelineStateManager pipelineMgr,
			IShaderManager shaderMgr,
			IExecutionPipeline pipeline,
			IEngine engine)
		{
			pProvider = provider;
			pLogger = logger;
			pRenderEvents = rendererEvts;
			pRenderState = renderState;
			pEngineEvents = events;
			pBufferProvider = bufferProvider;
			pPipelineMgr = pipelineMgr;
			pShaderMgr = shaderMgr;
			pExecutionPipeline = pipeline;
			pEngine = engine;

			pNeedsPrepareVar = pipeline.GetOrCreateVar(DefaultVars.RenderNeedsPrepare);

			events.OnStart += HandleEngineStart;
			events.OnBeforeStop += HandleEngineStop;
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if(pDisposed)
				return;

			pDisposed = true;

			pRenderEvents.ExecuteBeginDispose(this);

			pFeatureCollection.Dispose();
			SwapChain?.Dispose();
			
			pRenderEvents.ExecuteEndDispose(this);

			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnBeforeStop -= HandleEngineStop;
		}

		public IRenderer AddFeature(IRenderFeature feature, int zindex = -1)
		{
			AssertDispose();
			pFeatureCollection.AddFeature(feature, zindex);
			return this;
		}

		public IRenderer AddFeature(IEnumerable<IRenderFeature> features, int zindex = -1)
		{
			AssertDispose();
			pFeatureCollection.AddFeatures(features, zindex);
			return this;
		}

		public IRenderer RemoveFeature(IRenderFeature feature)
		{
			AssertDispose();
			pFeatureCollection.RemoveFeature(feature);
			return this;
		}

		public IRenderer Compile()
		{
			if (pDisposed || pDriver is null)
				return this;

			pNeedsPrepareVar.Value = pFeatureCollection.NeedsPrepare;

			RenderFeatureSetupInfo setupInfo = new RenderFeatureSetupInfo(
				pDriver,
				this,
				pBufferProvider,
				pPipelineMgr,
				pShaderMgr
			);

			foreach(var feature in pFeatureCollection)
			{
				if (!feature.IsDirty)
					continue;
				feature.Setup(setupInfo);
				feature.Compile(pDriver.ImmediateCommand);
			}

			return this;
		}

		public IRenderer Render()
		{
			// TODO: make this multi-thread
			if (pDisposed || pDriver is null)
				return this;
			
			// if swap chain has been setted, then we must clear
			if(pSwapChain != null)
			{
				var swapChainSize = pSwapChain.Size;

				UpdateFixedFrameBuffer();

				var colorBuffer = pSwapChain.ColorBuffer;
				pDriver.ImmediateCommand
					.SetRTs(new ITextureView[] { colorBuffer }, pSwapChain.DepthBuffer)
					.SetViewport(pRenderState.Viewport, swapChainSize.Width, swapChainSize.Height)
					.ClearRT(pSwapChain.ColorBuffer, pRenderState.DefaultClearColor)
					.ClearDepth(pSwapChain.DepthBuffer, pRenderState.ClearDepthFlags, pRenderState.DefaultClearDepthValue, pRenderState.DefaultClearStencilValue);

#if RENGINE_RENDERGRAPH
				if (pMainBackbufferResource is null)
					throw new EngineFatalException("Main Backbuffer Resource is null. It seems IRenderer does not filled this field");
				if (pMainDepthbufferResource is null)
					throw new EngineFatalException("Main Depthbuffer Resource is null. It seems IRenderer does not filled this field");
				
				pMainBackbufferResource.Value = colorBuffer;
				pMainDepthbufferResource.Value = pSwapChain.DepthBuffer;
#endif
			}

			foreach(var feature in pFeatureCollection)
			{
				if (feature.IsDirty)
					continue;

				feature.Execute(pDriver.ImmediateCommand);
			}
			return this;
		}

		public IRenderer PrepareFeatures()
		{
			if (pFeatureCollection.NeedsPrepare)
			{
				pLogger.Debug("Executing Render Prepare");
				pFeatureCollection.Prepare();
				pNeedsPrepareVar.Value = false;
				pLogger.Debug("Render Prepare Finished");
			}
			return this;
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException("IRenderer has been disposed.");
		}

		private void UpdateSwapChain(ISwapChain? swapChain)
		{
			if (swapChain == pSwapChain)
				return;

			if (pSwapChain != null)
				pSwapChain.OnResize -= HandleSwapChainResize;

			if((pSwapChain = swapChain) != null)
			{
				pSwapChain.OnResize += HandleSwapChainResize;
				var swapChainSize = pSwapChain.Size;

				UpdateFixedBufferData(swapChainSize);

				pRenderState.Viewport = new Viewport
				{
					Size = new Vector2(swapChainSize.Width,	 swapChainSize.Height)
				};
			}

			pLogger.Info("SwapChain has been changed.");
			pRenderEvents.ExecuteChangeSwapChain(this);
		}

		private void UpdateFixedBufferData(SwapChainSize size)
		{
			pLogger.Debug("Updating Fixed Buffer Data");

			Matrix4x4 proj = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0.0f, 1.0f);
			proj.M33 = proj.M43 = 0.5f;

			pRenderState.FrameData = new FrameData
			{
				ScreenProjection = proj,
				ScreenWidth = size.Width,
				ScreenHeight = size.Height,
				DeltaTime = (float)pEngine.DeltaTime,
				ElapsedTime = (float)pEngine.ElapsedTime,
			};

		}

		private void UpdateFixedFrameBuffer()
		{
			if (pDriver is null)
				return;

			var buffer = pBufferProvider.GetBuffer(BufferGroupType.Frame);
			var mappedData = pDriver.ImmediateCommand.Map<FrameData>(buffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = pRenderState.FrameData;
			pDriver.ImmediateCommand.Unmap(buffer, MapType.Write);
		}

		private void HandleBeginRender()
		{
			Compile();
		}

		private void HandleRender()
		{
			pRenderEvents.ExecuteBeginRender(this);

			Render();
			
			pRenderEvents.ExecuteEndRender(this);
		}

		private void HandlePresent()
		{
			pSwapChain?.Present(pRenderState.Vsync);
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pDriver = pProvider.GetOrDefault<IGraphicsDriver>();
			ISwapChain? swapChain = pProvider.GetOrDefault<ISwapChain>();
			
			pRenderEvents.ExecuteReady(this, pDriver);

			if (swapChain is null)
				pLogger.Warning("ISwapChain has not been setted on IRenderer, you must set a SwapChain to fully work IRenderer.");
			else
				UpdateSwapChain(swapChain);

#if RENGINE_RENDERGRAPH
			pRenderGraphResMgr = pProvider.Get<RenderGraph.IResourceManager>();
			pMainBackbufferResource = pRenderGraphResMgr.GetResource(ConstantRenderGraphNames.MainBackbufferResourceName);
			pMainDepthbufferResource = pRenderGraphResMgr.GetResource(ConstantRenderGraphNames.MainDepthbufferResourceName);
#endif

			pExecutionPipeline
				.AddEvent(DefaultEvents.RenderBeginId, (_) => HandleBeginRender())
				.AddEvent(DefaultEvents.RenderId, (_) => HandleRender())
				.AddEvent(DefaultEvents.RenderPrepareId, (_) => PrepareFeatures())
				.AddEvent(DefaultEvents.SwapChainPresentId, (_) => HandlePresent());
		}

		private void HandleSwapChainResize(object? sender, SwapChainResizeEventArgs e)
		{
			pLogger.Info("SwapChain resized. Updating Fixed Buffer and Viewport");
			UpdateFixedBufferData(e.Size);
			pRenderState.Viewport = new Viewport
			{
				Size = new Vector2(e.Size.Width, e.Size.Height)
			};
		}
	}
}
