using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Serialization;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Events;

namespace REngine.RPI
{
	internal class ShaderManagerImpl : IShaderManager, IDisposable
	{
		private readonly IServiceProvider pProvider;
		private readonly ShaderManagerEvents pShaderManagerEvents;

		private readonly ILogger<IShaderManager> pLogger;

		private readonly Dictionary<ulong, IShader> pShaders = new();

		private IDevice? pDevice;
		private bool pDisposed;


		public ShaderManagerImpl(
			ILoggerFactory loggerFactory,
			IServiceProvider provider,
			RendererEvents rendererEvents,
			ShaderManagerEvents shaderMgrEvents,
			PipelineStateManagerEvents pipelineStateEvents
		)
		{
			pProvider = provider;
			pShaderManagerEvents = shaderMgrEvents;
			pLogger = loggerFactory.Build<IShaderManager>();

			rendererEvents.OnBeforeReady.Once(HandleRendererReady);
			pipelineStateEvents.OnDisposed.Once(HandlePipelineStateDisposed);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pShaderManagerEvents.ExecuteDispose(this);
			pLogger.Info("Disposing Shader Manager");

			foreach (var pair in pShaders)
				pair.Value.Dispose();
			pShaders.Clear();

			pDisposed = true;
			pShaderManagerEvents.ExecuteDisposed(this);
		}
		
		private async Task HandlePipelineStateDisposed(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			Dispose();
		}

		private async Task HandleRendererReady(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			pLogger.Profile("Start Time");
			pDevice = pProvider.Get<IGraphicsDriver>().Device;

			LoadCache();

			pLogger.Info("Shader Manager started");

			pLogger.EndProfile("Start Time");
			pShaderManagerEvents.ExecuteReady(this);
		}

		private IDevice GetDevice()
		{
			return pDevice ?? throw new NullReferenceException("Device is null. Call this method after Engine Initialization");
		}

		public IShader? FindByHash(ulong hash)
		{
			if (hash == 0)
				return null;
			pShaders.TryGetValue(hash, out var shader);
			return shader;
		}

		public IShader GetOrCreate(ShaderCreateInfo createInfo)
		{
			ulong hash = createInfo.ToHash();
			IShader? shader = FindByHash(hash);
			if(shader != null) return shader;

			pShaders[hash] = shader = CreateShader(createInfo);
			pLogger.Debug($"Created Shader #{hash:X16}");
			SaveShader(hash, createInfo);
			return shader;
		}

		public IShaderManager ClearCache(bool clearFiles)
		{
			pShaders.Clear();
			return this;
		}

		private IShader CreateShader(ShaderCreateInfo createInfo)
		{
			return GetDevice().CreateShader(createInfo);
		}

		private void SaveShader(ulong hash, in ShaderCreateInfo createInfo)
		{
			if(!Directory.Exists(EngineSettings.ShaderCachePath))
				Directory.CreateDirectory(EngineSettings.ShaderCachePath);

			string shaderPath = Path.Combine(EngineSettings.ShaderCachePath, hash + ".rshader");
			if (File.Exists(shaderPath))
				return;

			using (FileStream stream = new FileStream(shaderPath, FileMode.CreateNew, FileAccess.Write))
			using (TextWriter writer = new StreamWriter(stream))
				writer.Write(createInfo.ToJson());
			pLogger.Debug($"Save Shader #{hash} to cache");
		}
		private void LoadCache()
		{
			if (!Directory.Exists(EngineSettings.ShaderCachePath))
			{
				Directory.CreateDirectory(EngineSettings.ShaderCachePath);
				return;
			}

			var files = Directory.GetFiles(EngineSettings.ShaderCachePath).Where(x => x.EndsWith(".rshader"));
			pLogger.Info($"Precompile ({files.Count()}) shaders");

			int skipped = 0;
			foreach(var file in files)
			{
				ShaderCreateInfo? ci;
				using(FileStream fileStream = new(file, FileMode.Open, FileAccess.Read))
				using(TextReader reader = new StreamReader(fileStream))
					ci = reader.ReadToEnd().FromJson<ShaderCreateInfo>();

				if (ci is null)
				{
					++skipped;
					continue;
				}

				var shader = CreateShader(ci.Value);
				pShaders[ci.Value.ToHash()] = shader;
			}

			pLogger.Info($"Loaded ({files.Count() - skipped}), Skipped ({skipped})");
		}
	}
}
