using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly IBufferProvider pBufferProvider;
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pExecutionPipeline;

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
			IBufferProvider bufferProvider,
			IExecutionPipeline pipeline)
		{
			pProvider = provider;
			pLogger = logger;
			pRenderEvents = rendererEvts;
			pRenderState = renderState;
			pEngineEvents = events;
			pBufferProvider = bufferProvider;
			pExecutionPipeline = pipeline;

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
				pBufferProvider
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
				pDriver.ImmediateCommand
					.SetRTs(new ITextureView[] { pSwapChain.ColorBuffer }, pSwapChain.DepthBuffer)
					.ClearRT(pSwapChain.ColorBuffer, pRenderState.DefaultClearColor)
					.ClearDepth(pSwapChain.DepthBuffer, pRenderState.ClearDepthFlags, pRenderState.DefaultClearDepthValue, pRenderState.DefaultClearStencilValue);
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
				pRenderState.FixedData = new RendererFixedData
				{
					ViewWidth = swapChainSize.Width,
					ViewHeight = swapChainSize.Height
				};
			}

			pLogger.Info("SwapChain has been changed.");
			pRenderEvents.ExecuteChangeSwapChain(this);

			UpdateFixedFrameBuffer(pSwapChain?.Size ?? new SwapChainSize());
		}

		private void UpdateFixedFrameBuffer(SwapChainSize size)
		{
			if (pDriver is null)
				return;

			var buffer = pBufferProvider.GetBuffer(BufferGroupType.Fixed);
			pDriver.ImmediateCommand.UpdateBuffer(buffer, 0, new RendererFixedData
			{
				ViewWidth = size.Width,
				ViewHeight = size.Height,
			});

			pLogger.Info("Updated Fixed Buffer");
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

			pExecutionPipeline
				.AddEvent(DefaultEvents.RenderBeginId, (_) => HandleBeginRender())
				.AddEvent(DefaultEvents.RenderId, (_) => HandleRender())
				.AddEvent(DefaultEvents.RenderPrepareId, (_) => PrepareFeatures())
				.AddEvent(DefaultEvents.SwapChainPresentId, (_) => HandlePresent());
		}

		private void HandleSwapChainResize(object? sender, SwapChainResizeEventArgs e)
		{
			UpdateFixedFrameBuffer(e.Size);
		}
	}
}
