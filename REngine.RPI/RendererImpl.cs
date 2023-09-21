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
		private IServiceProvider pProvider;
		private IGraphicsDriver? pDriver;
		private ILogger<IRenderer> pLogger;
		private RenderSettings pSetttings;
		private RendererEvents pRendererEvents;
		private RenderState pRenderState;

		private ISwapChain? pSwapChain;
		private IBuffer[] pBuffers = new IBuffer[(int)BufferGroupType.Object];

		private LinkedList<IRenderFeature> pFeatures = new LinkedList<IRenderFeature>();
		private LinkedList<IRenderFeature> pFeaturesToRemove = new LinkedList<IRenderFeature>();

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
			RendererEvents rendererEvts,
			RenderSettings settings,
			RenderState renderState)
		{
			pProvider = provider;
			pLogger = logger;
			pRendererEvents = rendererEvts;
			pSetttings = settings;
			pRenderState = renderState;

			events.OnStart += HandleEngineStart;
			events.OnBeforeStop += HandleEngineStop;
			events.OnBeginRender += HandleBeginRender;
			events.OnRender += HandleRender;
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			if(pDisposed) return;
			pDisposed = true;

			pRendererEvents.ExecuteBeginDispose(this);

			var feat = pFeatures.First;
			while(feat != null)
			{
				feat.Value.Dispose();
				feat = feat.Next;
			}
			pFeatures.Clear();
			
			SwapChain?.Dispose();
			pRendererEvents.ExecuteEndDispose(this);
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
				
				feature.Setup(pDriver, this);
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

		public IBuffer GetBuffer(BufferGroupType bufferType)
		{
			AssertDispose();

			IBuffer? buffer = pBuffers[GetBufferGroupIndex(bufferType)];
			if (buffer is null)
				throw new NullReferenceException("Buffer has not yet initialized.");

			return buffer;
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException("IRenderer has been disposed.");
		}

		private void UpdateSwapChain(ISwapChain? swapChain, bool updateBuffers = true)
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
			pRendererEvents.ExecuteChangeSwapChain(this);

			if(updateBuffers)
				UpdateBuffers(BufferUpdateFlags.Fixed);
		}

		private int GetBufferGroupIndex(BufferGroupType grp)
		{
			return ((int)grp) - 1;
		}
		private void UpdateBuffers(BufferUpdateFlags flags)
		{
			if (pDriver is null)
				throw new NullReferenceException("Driver has not been setted");
			bool executeEvent = flags != BufferUpdateFlags.None && pBuffers.Length > 0;

			BufferDesc bufferDesc = new BufferDesc
			{
				BindFlags = BindFlags.UniformBuffer,
				Usage = Usage.Immutable,
				AccessFlags = CpuAccessFlags.None
			};

			if((flags & BufferUpdateFlags.Fixed) != 0)
			{
				pBuffers[GetBufferGroupIndex(BufferGroupType.Fixed)]?.Dispose();

				bufferDesc.Name = "Fixed UBO";
				bufferDesc.Size = (ulong)Marshal.SizeOf<RendererFixedData>();

				pBuffers[GetBufferGroupIndex(BufferGroupType.Fixed)] = pDriver.Device.CreateBuffer(bufferDesc, pRenderState.FixedData);

				pLogger.Info("Fixed buffer has been changed");
			}

			bufferDesc.Usage = Usage.Dynamic;
			bufferDesc.AccessFlags = CpuAccessFlags.Write;
			bufferDesc.Mode = BufferMode.Raw;

			if((flags & BufferUpdateFlags.Frame) != 0)
			{
				pBuffers[GetBufferGroupIndex(BufferGroupType.Frame)]?.Dispose();

				bufferDesc.Name = "Frame UBO";
				bufferDesc.Size = pSetttings.FrameBufferSize;

				pBuffers[GetBufferGroupIndex(BufferGroupType.Frame)] = pDriver.Device.CreateBuffer(bufferDesc);

				pLogger.Info("Frame buffer has been changed");
			}

			if ((flags & BufferUpdateFlags.Object) != 0)
			{
				pBuffers[GetBufferGroupIndex(BufferGroupType.Object)]?.Dispose();

				bufferDesc.Name = "Object UBO";
				bufferDesc.Size = pSetttings.ObjectBufferSize;

				pBuffers[GetBufferGroupIndex(BufferGroupType.Object)] = pDriver.Device.CreateBuffer(bufferDesc);

				pLogger.Info("Object buffer has been changed");
			}

			if (executeEvent)
				pRendererEvents.ExecuteChangeBuffers(this);
		}

		private void HandleBeginRender(object sender, UpdateEventArgs args)
		{
			Compile();
		}

		private void HandleRender(object sender, UpdateEventArgs args)
		{
			if (pSwapChain is null)
				return;

			pRendererEvents.ExecuteBeginRender(this);

			Render();
			pSwapChain.Present(true);
			
			pRendererEvents.ExecuteEndRender(this);

			RemoveDisposedFeatures();
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

			if (swapChain is null)
				pLogger.Warning("ISwapChain has not been setted on IRenderer, you must set a SwapChain to fully work IRenderer.");
			else
				UpdateSwapChain(swapChain, false);

			UpdateBuffers(BufferUpdateFlags.All);
		}

		private void HandleSwapChainResize(object? sender, SwapChainResizeEventArgs e)
		{
			UpdateBuffers(BufferUpdateFlags.Fixed);
		}
	}
}
