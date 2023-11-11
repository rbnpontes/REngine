using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class PipelineStateManagerImpl : IPipelineStateManager, IDisposable
	{
		private readonly EngineEvents pEngineEvents;
		private readonly IServiceProvider pServiceProvider;
		private readonly ILogger<IPipelineStateManager> pLogger;

		private bool pDisposed;
		private IDevice? pDevice;

		private Dictionary<ulong, IPipelineState> pPipelines = new();
		private Dictionary<ulong, IComputePipelineState> pComputePipelines = new();

		public PipelineStateManagerImpl(
			EngineEvents engineEvents, 
			ILoggerFactory loggerFactory,
			IServiceProvider serviceProvider
		)
		{
			pLogger = loggerFactory.Build<IPipelineStateManager>();
			pEngineEvents = engineEvents;

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnStop += HandleEngineStop;
			pServiceProvider = serviceProvider;
		}

		public void Dispose()
		{
			if(pDisposed)
			{
				pLogger.Debug("Can´t dispose Pipeline State Manager because already disposed");
				return;
			}

			foreach(var pair in pPipelines)
				pair.Value.Dispose();
			pPipelines.Clear();

			pDisposed = true;
			GC.SuppressFinalize(this);

			pLogger.Info("Pipeline State has been disposed");
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			pLogger.Info("Stopping Pipeline State Manager");
			Dispose();
			pEngineEvents.OnStop -= HandleEngineStop;
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			pDevice = pServiceProvider.Get<IGraphicsDriver>().Device;

			pLogger.Info("Pipeline State Manager is Initialized");
		}

		private IDevice GetDevice()
		{
			return pDevice ?? throw new NullReferenceException("Device has not been loaded");
		}

		public IPipelineState GetOrCreate(GraphicsPipelineDesc desc)
		{
			ulong hash = desc.ToHash();
			IPipelineState? pipeline = FindGraphicsPipelineByHash(hash);
			if(pipeline != null)
				return pipeline;

			pipeline = GetDevice().CreateGraphicsPipeline(desc);
			pPipelines[hash] = pipeline;

			pLogger.Debug($"Created Graphics Pipeline #{hash.ToString("X16")}");
			return pipeline;
		}

		public IComputePipelineState GetOrCreate(ComputePipelineDesc desc)
		{
			ulong hash = desc.ToHash();
			IComputePipelineState? pipeline = FindComputePipelineByHash(hash);
			if (pipeline != null)
				return pipeline;

			pipeline = GetDevice().CreateComputePipeline(desc);
			pComputePipelines[hash] = pipeline;

			pLogger.Debug($"Created Compute Pipeline #{hash.ToString("X16")}");
			return pipeline;
        }

		public IPipelineState? FindGraphicsPipelineByHash(ulong hash)
		{
			pPipelines.TryGetValue(hash, out var pipeline);
			return pipeline;
		}
		public IComputePipelineState? FindComputePipelineByHash(ulong hash)
		{
			pComputePipelines.TryGetValue(hash, out var pipeline);
			return pipeline;
		}
	}
}
