﻿using REngine.Core;
using REngine.Core.IO;
using REngine.RHI;
using REngine.RPI.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class BufferProvider : IDisposable, IBufferProvider
	{
		private ILogger<IBufferProvider> pLogger;
		private bool pDisposed = false;

		private IGraphicsDriver? pDriver;
		private RenderSettings pRenderSettings;
		private RPIEvents pRPIEvents;

		private IBuffer[] pCBuffers = new IBuffer[(int)BufferGroupType.Object + 1];

		public BufferProvider(
			EngineEvents engineEvents, 
			RPIEvents rpiEvents,
			ILogger<IBufferProvider> logger,
			RenderSettings settings)
		{
			pRenderSettings = settings;
			pLogger = logger;
			pRPIEvents = rpiEvents;
			engineEvents.OnStop += HandleEngineStop;
			rpiEvents.OnReady += HandleRenderReady;
			rpiEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			if (pDisposed || pDriver is null)
				return;
			BuildBuffers();
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}
		private void HandleRenderReady(object? sender, RenderReadyEventArgs e)
		{
			pDriver = e.Driver;
			pLogger.Info("Initializing.");
			BuildBuffers();
			pLogger.Success("Initialized with success");
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pLogger.Info($"Disposing {nameof(IBufferProvider)}.");
			foreach(var cbuffer in pCBuffers)
			{
				cbuffer?.Dispose();
			}

			pDisposed = true;
		}

		public IBuffer GetBuffer(BufferGroupType groupType)
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(IBufferProvider));

			IBuffer buffer = pCBuffers[GetBufferGroupIndex(groupType)];
			if (buffer is null)
				throw new NullReferenceException($"Buffer of group type {groupType} is null. Did you forget to build buffers ?");
			return buffer;
		}

		public IBuffer GetInstancingBuffer(ulong bufferSize, bool dynamic)
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(IBufferProvider));
			if (pDriver is null)
				throw new NullReferenceException("Driver is required.");
			
			return pDriver.Device.CreateBuffer(new BufferDesc
			{
				Name = "Instancing Buffer",
				Size = bufferSize,
				BindFlags = BindFlags.VertexBuffer,
				Usage = dynamic ? Usage.Dynamic : Usage.Default,
				AccessFlags = CpuAccessFlags.Write
			});
		}

		private int GetBufferGroupIndex(BufferGroupType grpType)
		{
			return (int)grpType;
		}

		private void BuildBuffers()
		{
			if (pDriver is null)
				throw new NullReferenceException("Driver is required.");

			bool changed = false;

			BufferDesc desc = new BufferDesc
			{
				BindFlags = BindFlags.UniformBuffer,
				Usage = Usage.Default,
				Mode = BufferMode.Raw
			};

			int bufferIdx = -1;

			bufferIdx = GetBufferGroupIndex(BufferGroupType.Fixed);
			if(pCBuffers[bufferIdx]?.Size != (ulong)Marshal.SizeOf<RendererFixedData>())
			{
				changed = true;
				if (pCBuffers[bufferIdx] != null)
				{
					pLogger.Info("Disposing Fixed Buffer");
					pCBuffers[bufferIdx].Dispose();
				}

				pLogger.Info("Building Fixed Buffer");

				desc.Name = $"{nameof(IBufferProvider)} - Fixed CBuffer";
				desc.Size = (ulong)Marshal.SizeOf<RendererFixedData>();

				pCBuffers[bufferIdx] = pDriver.Device.CreateBuffer(desc);
				pLogger.Info("Fixed buffer has been created.");
			}

			desc.Usage = Usage.Dynamic;
			desc.AccessFlags = CpuAccessFlags.Write;

			bufferIdx = GetBufferGroupIndex(BufferGroupType.Frame);
			if(pCBuffers[bufferIdx]?.Size != pRenderSettings.FrameBufferSize)
			{
				changed = true;
				if (pCBuffers[bufferIdx] != null)
				{
					pLogger.Info("Disposing Frame Buffer");
					pCBuffers[bufferIdx].Dispose();
				}

				pLogger.Info("Building Frame Buffer");

				desc.Name = $"{nameof(IBufferProvider)} - Frame CBuffer";
				desc.Size = pRenderSettings.FrameBufferSize;

				pCBuffers[bufferIdx] = pDriver.Device.CreateBuffer(desc);
				pLogger.Info("Frame buffer has been created.");
			}

			bufferIdx = GetBufferGroupIndex(BufferGroupType.Object);
			if (pCBuffers[bufferIdx]?.Size != pRenderSettings.ObjectBufferSize)
			{
				changed = true;
				if (pCBuffers[bufferIdx] != null)
				{
					pLogger.Info("Disposing Object Buffer");
					pCBuffers[bufferIdx].Dispose();
				}

				pLogger.Info("Building Object Buffer");

				desc.Name = $"{nameof(IBufferProvider)} - Object CBuffer";
				desc.Size = pRenderSettings.ObjectBufferSize;

				pCBuffers[bufferIdx] = pDriver.Device.CreateBuffer(desc);
				pLogger.Info("Object buffer has been created.");
			}

			if (changed)
				pRPIEvents.ExecuteChangeBuffers(this);
		}
	}
}