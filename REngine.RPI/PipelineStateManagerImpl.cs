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

			SavePSCache(pServiceProvider.Get<IGraphicsDriver>());

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
			pEngineEvents.OnStop -= HandleEngineStop;
			pLogger.Info("Stopping Pipeline State Manager");
			Dispose();
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			var driver = pServiceProvider.Get<IGraphicsDriver>();
			pDevice = driver.Device;

			LoadPSCache(driver);
			LoadCacheItems();

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

		private IPipelineState CreatePipeline(GraphicsPipelineDesc desc)
		{
			desc.PSCache ??= PSCache;
			return GetDevice().CreateGraphicsPipeline(desc);
		}

		private IComputePipelineState CreatePipeline(ComputePipelineDesc desc)
		{
			desc.PSCache ??= PSCache;
			return GetDevice().CreateComputePipeline(desc);
		}

		private void LoadPSCache(IGraphicsDriver driver)
		{
			if (driver.Backend is not (GraphicsBackend.D3D12 or GraphicsBackend.Vulkan))
				return;

			if (!File.Exists(EngineSettings.PipelineCachePath))
			{
				PSCache = GetDevice().CreatePipelineStateCache();
				return;
			}

			pLogger.Info("Loading Pipeline State Cache");

			byte[] data;
			using (var stream = new FileStream(EngineSettings.PipelineCachePath, FileMode.Open, FileAccess.Read))
			{
				using PipelineStateCacheDeserializer deserializer = new(stream);
				deserializer.Deserialize(driver, out data);
			}

			PSCache = GetDevice().CreatePipelineStateCache(data);
			pLogger.Info("Loaded Pipeline State Cache");
		}

		private void SavePSCache(IGraphicsDriver driver)
		{
			if (PSCache is null)
				return;

			pLogger.Info("Saving Pipeline State Cache");


			if (File.Exists(EngineSettings.PipelineCachePath))
				File.Delete(EngineSettings.PipelineCachePath);

			using (FileStream stream = new(EngineSettings.PipelineCachePath, FileMode.CreateNew, FileAccess.Write))
			{
				using PipelineStateCacheSerializer serializer = new(PSCache, stream);
				serializer.Serialize(driver);
			}

			pLogger.Success("Pipeline State Cache has been saved");
		}

		private void LoadCacheItems()
		{
			if (!File.Exists(EngineSettings.PipelineItemsPath))
			{
				InitSerializer();
				return;
			}

			GraphicsPipelineDesc[] graphics2Create;
			ComputePipelineDesc[] compute2Create;
			using (FileStream stream = new(EngineSettings.PipelineItemsPath, FileMode.Open, FileAccess.Read))
			{
				using PipelineDeserializer deserializer = new(stream);
				deserializer.Deserialize(pShaderManager);

				graphics2Create = deserializer.GetGraphicsDescriptions();
				compute2Create = deserializer.GetComputeDescriptions();
			}
			
			var serializer = InitSerializer();

			pLogger.Info(
				$"Building Cached Pipelines. Graphics Pipelines ({graphics2Create.Length}), Compute Pipelines ({compute2Create.Length})");

			foreach (var graphicsPipelineDesc in graphics2Create)
			{
				var hash = graphicsPipelineDesc.ToHash();
				pPipelines[hash] = CreatePipeline(graphicsPipelineDesc);
				serializer.AddDesc(graphicsPipelineDesc);
			}

			foreach (var computePipelineDesc in compute2Create)
			{
				var hash = computePipelineDesc.ToHash();
				pComputePipelines[hash] = CreatePipeline(computePipelineDesc);
				serializer.AddDesc(computePipelineDesc);
			}

			pLogger.Success("Success at loading cached pipelines");
		}

		private PipelineSerializer InitSerializer()
		{
			if (File.Exists(EngineSettings.PipelineItemsPath))
				File.Delete(EngineSettings.PipelineItemsPath);
			return pSerializer = new PipelineSerializer(pSerializerStream = new FileStream(EngineSettings.PipelineItemsPath, FileMode.Create, FileAccess.Write));
		}
	}
}
