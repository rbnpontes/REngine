using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Serialization;

namespace REngine.RPI
{
	internal class PipelineStateManagerImpl : IPipelineStateManager, IDisposable
	{

		private readonly EngineEvents pEngineEvents;
		private readonly IServiceProvider pServiceProvider;
		private readonly ILogger<IPipelineStateManager> pLogger;
		private readonly IShaderManager pShaderManager;

		private bool pDisposed;
		private IDevice? pDevice;

		private readonly Dictionary<ulong, IPipelineState> pPipelines = new();
		private readonly Dictionary<ulong, IComputePipelineState> pComputePipelines = new();

		private PipelineSerializer? pSerializer;
		private Stream? pSerializerStream;
		public IPipelineStateCache? PSCache { get; private set; }

		public PipelineStateManagerImpl(
			EngineEvents engineEvents, 
			ILoggerFactory loggerFactory,
			IServiceProvider serviceProvider,
			IShaderManager shaderManager
		)
		{
			pLogger = loggerFactory.Build<IPipelineStateManager>();
			pEngineEvents = engineEvents;

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnStop += HandleEngineStop;
			pServiceProvider = serviceProvider;
			pShaderManager = shaderManager;
		}

		public void Dispose()
		{
			if(pDisposed)
			{
				pLogger.Debug("Can´t dispose Pipeline State Manager because already disposed");
				return;
			}

			SavePSCache();

			pSerializer?.Dispose();
			pSerializer = null;

			pSerializerStream?.Dispose();
			pSerializerStream = null;

			foreach(var pair in pPipelines)
				pair.Value.Dispose();
			pPipelines.Clear();

			pDisposed = true;

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
			var driver = pServiceProvider.Get<IGraphicsDriver>();
			pDevice = driver.Device;

			LoadPSCache(driver.Backend);
			LoadCacheItems();

			if(File.Exists(EngineSettings.PipelineItemsPath))
				File.Delete(EngineSettings.PipelineItemsPath);

			pSerializer = new PipelineSerializer(pSerializerStream = new FileStream(EngineSettings.PipelineItemsPath, FileMode.Create, FileAccess.Write));
			pLogger.Info("Pipeline State Manager is Initialized");
		}

		private IDevice GetDevice()
		{
			return pDevice ?? throw new NullReferenceException("Device has not been loaded");
		}

		public IPipelineState GetOrCreate(GraphicsPipelineDesc desc)
		{
			var hash = desc.ToHash();
			var pipeline = FindGraphicsPipelineByHash(hash);
			if(pipeline != null)
				return pipeline;

			pipeline = CreatePipeline(desc);
			pPipelines[hash] = pipeline;

			pSerializer?.AddDesc(desc);

			pLogger.Debug($"Created Graphics Pipeline #{hash:X16}");
			return pipeline;
		}

		public IComputePipelineState GetOrCreate(ComputePipelineDesc desc)
		{
			var hash = desc.ToHash();
			var pipeline = FindComputePipelineByHash(hash);
			if (pipeline != null)
				return pipeline;

			pipeline = CreatePipeline(desc);
			pComputePipelines[hash] = pipeline;

			pSerializer?.AddDesc(desc);

			pLogger.Debug($"Created Compute Pipeline #{hash:X16}");
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

		private void LoadPSCache(GraphicsBackend backend)
		{
			if (backend is not (GraphicsBackend.D3D12 or GraphicsBackend.Vulkan))
				return;

			if (!File.Exists(EngineSettings.PipelineCachePath))
			{
				PSCache = GetDevice().CreatePipelineStateCache();
				return;
			}

			pLogger.Info("Loading Pipeline State Cache");
			var data = File.ReadAllBytes(EngineSettings.PipelineCachePath);
			PSCache = GetDevice().CreatePipelineStateCache(data);
			pLogger.Info("Loaded Pipeline State Cache");
		}
	
		private void SavePSCache()
		{
			if (PSCache is null)
				return;

			pLogger.Info("Saving Pipeline State Cache");

			PSCache.GetData(out var data);

			if(File.Exists(EngineSettings.PipelineCachePath))
				File.Delete(EngineSettings.PipelineCachePath);

			File.WriteAllBytes(EngineSettings.PipelineCachePath, data);
			pLogger.Success("Pipeline State Cache has been saved");
		}

		private IPipelineState CreatePipeline(GraphicsPipelineDesc desc)
		{
			return GetDevice().CreateGraphicsPipeline(desc);
		}

		private IComputePipelineState CreatePipeline(ComputePipelineDesc desc)
		{
			return GetDevice().CreateComputePipeline(desc);
		}

		private void LoadCacheItems()
		{
			if (!File.Exists(EngineSettings.PipelineItemsPath))
				return;

			using FileStream stream = new (EngineSettings.PipelineItemsPath, FileMode.Open, FileAccess.Read);
			using PipelineDeserializer deserializer = new(stream);
			deserializer.Deserialize(pShaderManager);

			var graphics2Create = deserializer.GetGraphicsDescriptions();
			var compute2Create = deserializer.GetComputeDescriptions();

			pLogger.Info(
				$"Building Cached Pipelines. Graphics Pipelines ({graphics2Create.Length}), Compute Pipelines ({compute2Create.Length})");

			foreach (var graphicsPipelineDesc in graphics2Create)
			{
				var hash = graphicsPipelineDesc.ToHash();
				pPipelines[hash] = CreatePipeline(graphicsPipelineDesc);
			}

			foreach (var computePipelineDesc in compute2Create)
			{
				var hash = computePipelineDesc.ToHash();
				pComputePipelines[hash] = CreatePipeline(computePipelineDesc);
			}

			pLogger.Success("Success at loading cached pipelines");
		}
	}
}
