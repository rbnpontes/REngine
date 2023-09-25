using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
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

		private bool pDisposed = false;
		private readonly IServiceProvider pProvider;
		private readonly ILogger<IRenderer> pLogger;
		private readonly RPIEvents pRenderEvents;
		private readonly RenderState pRenderState;
		private readonly IBufferProvider pBufferProvider;
		private readonly EngineEvents pEngineEvents;

		private readonly LinkedList<IRenderFeature> pFeatures = new();
		private readonly LinkedList<IRenderFeature> pFeaturesToRemove = new();

		private IGraphicsDriver? pDriver;

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
			IBufferProvider bufferProvider)
		{
			pProvider = provider;
			pLogger = logger;
			pRenderEvents = rendererEvts;
			pRenderState = renderState;

			events.OnStart += HandleEngineStart;
			events.OnBeforeStop += HandleEngineStop;
			events.OnBeginRender += HandleBeginRender;
			events.OnRender += HandleRender;
			events.OnAsyncRender += HandleAsyncRender;

			pEngineEvents = events;
			pBufferProvider = bufferProvider;
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if(pDisposed) return;
			pDisposed = true;

			pRenderEvents.ExecuteBeginDispose(this);

			var feat = pFeatures.First;
			while(feat != null)
			{
				feat.Value.Dispose();
				feat = feat.Next;
			}
			pFeatures.Clear();
			
			SwapChain?.Dispose();
			pRenderEvents.ExecuteEndDispose(this);

			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnBeforeStop -= HandleEngineStop;
			pEngineEvents.OnBeginRender -= HandleBeginRender;
			pEngineEvents.OnRender -= HandleRender;
			pEngineEvents.OnAsyncRender -= HandleAsyncRender;
		}

		public IRenderer AddFeature(IRenderFeature feature)
		{
			AssertDispose();
			pFeatures.AddLast(feature);
			return this;
		}

		public IRenderer AddFeature(IEnumerable<IRenderFeature> features)
		{
			AssertDispose();
			foreach (var feat in features)
				AddFeature(feat);
			return this;
		}

		public IRenderer RemoveFeature(IRenderFeature feature)
		{
			AssertDispose();
			pFeatures.Remove(feature);
			return this;
		}

		public IRenderer Compile()
		{
			if (pDisposed || pDriver is null)
				return this;
			var featNode = pFeatures.First;
			RenderFeatureSetupInfo setupInfo = new RenderFeatureSetupInfo(
				pDriver,
				this,
				pBufferProvider
			);
			while(featNode != null)
			{
				var feature = featNode.Value;
				if (feature.IsDisposed)
				{
					pFeaturesToRemove.AddLast(feature);
					featNode = featNode.Next;
					continue;
				}

				if (!feature.IsDirty)
				{
					featNode = featNode.Next;
					continue;
				}
				
				feature.Setup(setupInfo);
				feature.Compile(pDriver.ImmediateCommand);

				featNode = featNode.Next;
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

			var featNode = pFeatures.First;
			while (featNode != null)
			{
				if(featNode.Value.IsDisposed || featNode.Value.IsDirty)
				{
					featNode = featNode.Next;
					continue;
				}

				featNode.Value.Execute(pDriver.ImmediateCommand);
				featNode = featNode.Next;
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

		private void HandleBeginRender(object? sender, UpdateEventArgs args)
		{
			Compile();
		}

		private void HandleRender(object? sender, UpdateEventArgs args)
		{
			pRenderEvents.ExecuteBeginRender(this);

			Render();
			
			pRenderEvents.ExecuteEndRender(this);

			RemoveDisposedFeatures();
		}

		private void HandleAsyncRender(object? sender, EventArgs e)
		{
			pSwapChain?.Present(true);
		}

		private void RemoveDisposedFeatures()
		{
			// TODO: improve this remove logic
			var next = pFeaturesToRemove.First;
			while(next != null)
			{
				pFeatures.Remove(next.Value);
				next = next.Next;
			}
			pFeaturesToRemove.Clear();
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
		}

		private void HandleSwapChainResize(object? sender, SwapChainResizeEventArgs e)
		{
			UpdateFixedFrameBuffer(e.Size);
		}
	}
}
