using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Events;
using REngine.RPI.Resources;
using REngine.RPI.Serialization;

namespace REngine.RPI
{
	internal class PipelineStateManagerImpl : IPipelineStateManager, IDisposable
	{
		private readonly IServiceProvider pServiceProvider;
		private readonly ILogger<IPipelineStateManager> pLogger;
		private readonly IShaderManager pShaderManager;
		private readonly PipelineStateManagerEvents pPipelineStateEvents;
		private readonly RendererEvents pRendererEvents;
		private readonly ShaderManagerEvents pShaderMgrEvents;

		private readonly Dictionary<ulong, IPipelineState> pPipelines = new();
		private readonly Dictionary<ulong, IComputePipelineState> pComputePipelines = new();
		
		private bool pDisposed;
		private IDevice? pDevice;

		private PipelineSerializer? pSerializer;
		private bool pSaveCache;

		public IPipelineStateCache? PSCache { get; private set; }

		public PipelineStateManagerImpl(
			ILoggerFactory loggerFactory,
			IServiceProvider serviceProvider,
			IShaderManager shaderManager,
			PipelineStateManagerEvents pipelineEvents,
			RendererEvents rendererEvents,
			ShaderManagerEvents shaderMgrEvents
		)
		{
			pLogger = loggerFactory.Build<IPipelineStateManager>();
			pServiceProvider = serviceProvider;
			pShaderManager = shaderManager;
			pPipelineStateEvents = pipelineEvents;
			pRendererEvents = rendererEvents;
			pShaderMgrEvents = shaderMgrEvents;

			//engineEvents.OnStart += HandleEngineStart;
			//engineEvents.OnStop += HandleEngineStop;
			shaderMgrEvents.OnReady.Once(HandleShaderManagerReady);
			rendererEvents.OnDisposed.Once(HandleRendererDisposed);
		}

		public void Dispose()
		{
			if(pDisposed)
			{
				pLogger.Debug("Can´t dispose Pipeline State Manager because already disposed");
				return;
			}

			pPipelineStateEvents.ExecuteDispose(this);
			// Only save cache if pipeline state list has been changed, this means that new items have been added
			// only writes to disk if it needed
			if (pSaveCache)
			{
				SavePSCache(pServiceProvider.Get<IGraphicsDriver>());
				SaveCachedItems();
			}

			foreach(var pair in pPipelines)
				pair.Value.Dispose();
			pPipelines.Clear();

			pDisposed = true;

			pLogger.Info("Pipeline State has been disposed");
			pPipelineStateEvents.ExecuteDisposed(this);
		}

		private async Task HandleShaderManagerReady(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			pLogger.Profile("Start Time");

			var driver = pServiceProvider.Get<IGraphicsDriver>();
			pDevice = driver.Device;

			LoadPSCache(driver);
			LoadCacheItems();

			pLogger.Info("Pipeline State Manager is Initialized");

			pLogger.EndProfile("Start Time");
			await pPipelineStateEvents.ExecuteReady(this);
		}

		private async Task HandleRendererDisposed(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			Dispose();
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

			pSaveCache = true;
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

			pSaveCache = true;
			pipeline = CreatePipeline(desc);
			pComputePipelines[hash] = pipeline;

			pSerializer?.AddDesc(desc);

			pLogger.Debug($"Created Compute Pipeline #{hash:X16}");
			return pipeline;
        }

		public IComputePipelineState CreateComputeFromShader(ShaderAsset asset)
		{
			return GetOrCreate(new ComputePipelineDesc
			{
				Name = "Compute Pipeline: "+asset.Name,
				ComputeShader = asset.BuildShader(ShaderType.Compute)
			});
		}
		
		public IComputePipelineState CreateComputeFromShader(IShader shader)
		{
			if (shader.Type != ShaderType.Compute)
				throw new ArgumentException($"Shader Type must be of {nameof(ShaderType.Compute)}");
			return GetOrCreate(new ComputePipelineDesc
			{
				Name = "Compute Pipeline: "+shader.Name,
				ComputeShader = shader
			});
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

		private void SaveCachedItems()
		{
			if(File.Exists(EngineSettings.PipelineItemsPath))
				File.Delete(EngineSettings.PipelineItemsPath);

			using FileStream stream = new(EngineSettings.PipelineItemsPath, FileMode.CreateNew, FileAccess.Write);
			pSerializer?.Serialize(stream);
		}
		private PipelineSerializer InitSerializer()
		{
			return pSerializer = new PipelineSerializer();
		}

	}
}
