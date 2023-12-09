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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;
using REngine.RPI.Events;

namespace REngine.RPI
{
	internal class RendererImpl : IRenderer
	{
		private readonly IServiceProvider pProvider;
		private readonly ILogger<IRenderer> pLogger;
		private readonly RendererEvents pRenderEvents;
		private readonly RenderState pRenderState;
		private readonly GraphicsSettings pGraphicsSettings;

		#region Managers
		private readonly IBufferManager pBufferProvider;
		private readonly IPipelineStateManager pPipelineMgr;
		private readonly IShaderManager pShaderMgr;
		private readonly IRenderTargetManager pRenderTargetMgr;
		private readonly IAssetManager pAssetManager;
		#endregion

		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly IEngine pEngine;

		private readonly Action<IRenderFeature> pCompileAction;
		private readonly Action<IRenderFeature> pRenderAction;

#if RENGINE_RENDERGRAPH
		private RenderGraph.IResourceManager? pRenderGraphResMgr;
		private RenderGraph.IResource? pMainBackBufferResource;
		private RenderGraph.IResource? pMainDepthBufferResource;
#endif
		private readonly IExecutionPipelineVar pNeedsPrepareVar;

		private readonly FeatureCollection pFeatureCollection = new();

		private RenderFeatureSetupInfo pSetupInfo;

		private IGraphicsDriver? pDriver;

		private ISwapChain? pSwapChain;

		public bool IsDisposed { get; private set; }

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
			RendererEvents rendererEvents,
			RenderState renderState,
			IBufferManager bufferProvider,
			IPipelineStateManager pipelineMgr,
			IShaderManager shaderMgr,
			IRenderTargetManager renderTargetMgr,
			IExecutionPipeline pipeline,
			IEngine engine,
			GraphicsSettings graphicsSettings,
			IAssetManager assetManager)
		{
			pProvider = provider;
			pLogger = logger;
			pRenderEvents = rendererEvents;
			pRenderState = renderState;
			pEngineEvents = events;
			pBufferProvider = bufferProvider;
			pPipelineMgr = pipelineMgr;
			pShaderMgr = shaderMgr;
			pRenderTargetMgr = renderTargetMgr;
			pExecutionPipeline = pipeline;
			pEngine = engine;
			pGraphicsSettings = graphicsSettings;
			pAssetManager = assetManager;

			pNeedsPrepareVar = pipeline.GetOrCreateVar(DefaultVars.RenderNeedsPrepare);

			events.OnStart += HandleEngineStart;
			events.OnBeforeStop += HandleEngineStop;

			pCompileAction = ExecCompile;
			pRenderAction = ExecRender;
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if(IsDisposed)
				return;

			IsDisposed = true;

			pRenderEvents.ExecuteDispose(this);

			pFeatureCollection.Dispose();
			SwapChain?.Dispose();
			
			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnBeforeStop -= HandleEngineStop;

			pRenderEvents.ExecuteDisposed(this);
		}

		public IRenderer AddFeature(IRenderFeature feature, int zindex = -1)
		{
			if (IsDisposed)
				return this;
			pFeatureCollection.AddFeature(feature, zindex);
			return this;
		}

		public IRenderer AddFeature(IEnumerable<IRenderFeature> features, int zindex = -1)
		{
			if (IsDisposed)
				return this;
			pFeatureCollection.AddFeatures(features, zindex);
			return this;
		}

		public IRenderer RemoveFeature(IRenderFeature feature)
		{
			if (IsDisposed)
				return this;
			pFeatureCollection.RemoveFeature(feature);
			return this;
		}

		public IRenderer Compile()
		{
			if (IsDisposed || pDriver is null)
				return this;

			pNeedsPrepareVar.Value = pFeatureCollection.NeedsPrepare;
			pFeatureCollection.ForEach(pCompileAction);
			return this;

		}
		private void ExecCompile(IRenderFeature feature)
		{
			if (!feature.IsDirty || pDriver is null)
				return;

			pRenderEvents.ExecuteBeginCompile(this);
			feature.Setup(pSetupInfo);
			feature.Compile(pDriver.ImmediateCommand);
			pRenderEvents.ExecuteEndCompile(this);
		}

		public IRenderer Render()
		{
			// TODO: make this multi-thread
			if (IsDisposed || pDriver is null)
				return this;
			
			// if swap chain has been set, then we must clear
			if(pSwapChain != null)
			{
				var swapChainSize = pSwapChain.Size;

				UpdateFixedFrameBuffer();

				var colorBuffer = pSwapChain.ColorBuffer;
				pDriver.ImmediateCommand
					.SetRT(colorBuffer, pSwapChain.DepthBuffer)
					.SetViewport(pRenderState.Viewport, swapChainSize.Width, swapChainSize.Height)
					.ClearRT(pSwapChain.ColorBuffer, pRenderState.DefaultClearColor)
					.ClearDepth(pSwapChain.DepthBuffer, pRenderState.ClearDepthFlags, pRenderState.DefaultClearDepthValue, pRenderState.DefaultClearStencilValue);

#if RENGINE_RENDERGRAPH
				if (pMainBackBufferResource is null)
					throw new EngineFatalException("Main BackBuffer Resource is null. It seems IRenderer does not filled this field");
				if (pMainDepthBufferResource is null)
					throw new EngineFatalException("Main DepthBuffer Resource is null. It seems IRenderer does not filled this field");
				
				pMainBackBufferResource.Value = colorBuffer;
				pMainDepthBufferResource.Value = pSwapChain.DepthBuffer;
#endif
			}

			pFeatureCollection.ForEach(pRenderAction);
			return this;
		}

		private void ExecRender(IRenderFeature feature)
		{
			if (feature.IsDirty || pDriver is null)
				return;
			feature.Execute(pDriver.ImmediateCommand);
		}

		public IRenderer PrepareFeatures()
		{
			if (!pFeatureCollection.NeedsPrepare.Value)
				return this;
			pLogger.Debug("Executing Render Prepare");
			pFeatureCollection.Prepare();
			pNeedsPrepareVar.Value = false;
			pLogger.Debug("Render Prepare Finished");
			return this;
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
			pRenderEvents.ExecuteChangeSwapChain(this, pSwapChain);
		}

		private void UpdateFixedBufferData(SwapChainSize size)
		{
			if (pDriver is null)
				throw new NullReferenceException("Driver is required");
			pLogger.Debug("Updating Fixed Buffer Data");

			var proj = Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, size.Width, size.Height, 0, 0.0f, 1.0f);
			Matrix4x4.Invert(proj, out var invProj);

			pRenderState.FrameData = new FrameData
			{
				ScreenProjection = proj,
				InvScreenProjection = invProj,
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

			var frameData = pRenderState.FrameData;
			frameData.DeltaTime = (float)pEngine.DeltaTime;
			frameData.ElapsedTime = (float)pEngine.ElapsedTime;

			var buffer = pBufferProvider.GetBuffer(BufferGroupType.Frame);
			var mappedData = pDriver.ImmediateCommand.Map<FrameData>(buffer, MapType.Write, MapFlags.Discard);
			mappedData[0] = frameData;
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
			pLogger.Profile("Start Time");
			pDriver = pProvider.GetOrDefault<IGraphicsDriver>();
			var swapChain = pProvider.GetOrDefault<ISwapChain>();
			
			if (swapChain is null)
				pLogger.Warning("ISwapChain has not been set on IRenderer, you must set a SwapChain to fully work IRenderer.");
			else
				UpdateSwapChain(swapChain);

#if RENGINE_RENDERGRAPH
			pRenderGraphResMgr = pProvider.Get<RenderGraph.IResourceManager>();
			pMainBackBufferResource = pRenderGraphResMgr.GetResource(ConstantRenderGraphNames.MainBackbufferResourceName);
			pMainDepthBufferResource = pRenderGraphResMgr.GetResource(ConstantRenderGraphNames.MainDepthbufferResourceName);
#endif


			pSetupInfo = new RenderFeatureSetupInfo(
				pDriver,
				this,
				pBufferProvider,
				pPipelineMgr,
				pShaderMgr,
				pRenderTargetMgr,
				pGraphicsSettings,
				pRenderState,
				pAssetManager
			);

			pExecutionPipeline
				.AddEvent(DefaultEvents.RenderBeginId, (_) => HandleBeginRender())
				.AddEvent(DefaultEvents.RenderId, (_) => HandleRender())
				.AddEvent(DefaultEvents.RenderPrepareId, (_) => PrepareFeatures());
// #if !ANDROID // Disable SwapChain present on Android, this must be done on Android Main Thread
// #endif
			pExecutionPipeline.AddEvent(DefaultEvents.SwapChainPresentId, (_) => HandlePresent());

			pLogger.EndProfile("Start Time");
			pRenderEvents.ExecuteBeforeReady(this);
			pRenderEvents.ExecuteReady(this);
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
