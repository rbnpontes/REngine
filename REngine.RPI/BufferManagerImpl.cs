﻿using REngine.Core;
using REngine.Core.IO;
using REngine.Core.SceneManagement;
using REngine.RHI;
using REngine.RPI.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Events;

namespace REngine.RPI
{
	internal class BufferManagerImpl : IDisposable, IBufferManager
	{
		private readonly ILogger<IBufferManager> pLogger;
		private readonly RendererEvents pRendererEvents;
		private readonly BufferManagerEvents pBufferMgrEvents;
		private readonly RenderSettings pRenderSettings;
		private readonly RPIEvents pRpiEvents;
		private readonly IBuffer[] pCBuffers = new IBuffer[(int)BufferGroupType.Material];

		private bool pDisposed;

		private IGraphicsDriver? pDriver;
		
		public BufferManagerImpl(
			ILogger<IBufferManager> logger,
			RendererEvents rendererEvents,
			BufferManagerEvents bufferMgrEvents,
			RPIEvents rpiEvents,
			RenderSettings settings)
		{
			pLogger = logger;
			pRenderSettings = settings;
			pBufferMgrEvents = bufferMgrEvents;
			pRendererEvents = rendererEvents;
			pBufferMgrEvents = bufferMgrEvents;
			pRpiEvents = rpiEvents;

			rendererEvents.OnReady.Once(HandleRendererReady);
			rendererEvents.OnDisposed.Once(HandleRendererDisposed);
			rpiEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private async Task HandleRendererReady(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			if (sender is not IRenderer renderer)
				return;

			pLogger.Profile("Start Time");

			pDriver = renderer.Driver;

			pLogger.Info("Initializing.");
			BuildBuffers();
			pLogger.Success("Initialized with success");

			pBufferMgrEvents.ExecuteReady(this);

			pLogger.EndProfile("Start Time");
		}

		private async Task HandleRendererDisposed(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			Dispose();
		}

		private void HandleUpdateSettings(object? sender, EventArgs e)
		{
			if (pDisposed || pDriver is null)
				return;
			BuildBuffers();
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pRpiEvents.OnUpdateSettings -= HandleUpdateSettings;
			pBufferMgrEvents.ExecuteDispose(this);

			pLogger.Info($"Disposing {nameof(IBufferManager)}.");
			foreach(var buffer in pCBuffers)
				buffer?.Dispose();

			pDisposed = true;

			pBufferMgrEvents.ExecuteDisposed(this);
		}

		public IBuffer GetBuffer(BufferGroupType groupType)
		{
			ObjectDisposedException.ThrowIf(pDisposed, this);

			var buffer = pCBuffers[GetBufferGroupIndex(groupType)];
			return buffer ?? throw new NullReferenceException(
				$"Buffer of group type {groupType} is null. Did you forget to build buffers ?");
		}

		public IBuffer GetInstancingBuffer(ulong bufferSize, bool dynamic)
		{
			return GetDriver().Device.CreateBuffer(new BufferDesc
			{
				Name = "Instancing Buffer",
				Size = bufferSize,
				BindFlags = BindFlags.VertexBuffer,
				Usage = dynamic ? Usage.Dynamic : Usage.Default,
				AccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None
			});
		}

		public IBuffer Allocate(BufferDesc desc)
		{
			return GetDriver().Device.CreateBuffer(desc);
		}

		public IBuffer Allocate<T>(BufferDesc desc, T[] data) where T : unmanaged
		{
			return GetDriver().Device.CreateBuffer(desc, data);
		}

		public IBuffer Allocate<T>(BufferDesc desc, T data) where T : unmanaged
		{
			return GetDriver().Device.CreateBuffer(desc, data);
		}
		
		private static int GetBufferGroupIndex(BufferGroupType grpType)
		{
			return (int)(grpType - 1);
		}

		private IGraphicsDriver GetDriver()
		{
			ObjectDisposedException.ThrowIf(pDisposed, this);
			if (pDriver is null)
				throw new NullReferenceException("Driver is required.");
			return pDriver;
		}
		private void BuildBuffers()
		{
			if (pDriver is null)
				throw new NullReferenceException("Driver is required.");

			bool changed = false;

			BufferDesc desc = new BufferDesc
			{
				BindFlags = BindFlags.UniformBuffer,
				Usage = Usage.Dynamic,
				AccessFlags = CpuAccessFlags.Write
			};

			ulong[] bufferSizes = {
				(ulong)Marshal.SizeOf<FrameData>(),
				(ulong)Marshal.SizeOf<CameraData>(),
				pRenderSettings.ObjectBufferSize,
				pRenderSettings.MaterialBufferSize
			};

			for(int i =0; i < bufferSizes.Length; ++i)
			{
				BufferGroupType type = (BufferGroupType)(i + 1);
				if (pCBuffers[i]?.Size == bufferSizes[i])
					continue;
				changed = true;
				if (pCBuffers[i] != null)
				{
					pLogger.Info($"Disposing {type} Buffer");
					pCBuffers[i].Dispose();
				}

				pLogger.Info($"Building {type} Buffer");

				desc.Name = $"{nameof(IBufferManager)} - {type} CBuffer";
				desc.Size = bufferSizes[i];
				
				pCBuffers[i] = pDriver.Device.CreateBuffer(desc);
				pLogger.Info($"{type} buffer has been created.");
			}

			if (changed)
				pBufferMgrEvents.ExecuteChange(this);
		}
	}
}
