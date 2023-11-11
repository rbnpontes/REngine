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

namespace REngine.RPI
{
	internal class ShaderManagerImpl : IShaderManager, IDisposable
	{
		private readonly IServiceProvider pProvider;
		private readonly EngineEvents pEngineEvents;
		private readonly ILogger<IShaderManager> pLogger;

		private readonly Dictionary<ulong, IShader> pShaders = new();

		private IDevice? pDevice;
		private bool pDisposed;


		public ShaderManagerImpl(
			ILoggerFactory loggerFactory,
			IServiceProvider provider,
			EngineEvents engineEvents
		)
		{
			pProvider = provider;
			pEngineEvents = engineEvents;
			pLogger = loggerFactory.Build<IShaderManager>();

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnStop += HandleEngineStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			pLogger.Info("Disposing Shader Manager");

			foreach (var pair in pShaders)
				pair.Value.Dispose();
			pShaders.Clear();

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			pEngineEvents.OnStop -= HandleEngineStop;
			Dispose();
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;

			pDevice = pProvider.Get<IDevice>();
			
			LoadCache();

			pLogger.Info("Shader Manager started");
		}

		private IDevice GetDevice()
		{
			return pDevice ?? throw new NullReferenceException("Device is null. Call this method after Engine Initialization");
		}

		public IShader? FindByHash(ulong hash)
		{
			pShaders.TryGetValue(hash, out var shader);
			return shader;
		}

		public IShader GetOrCreate(ShaderCreateInfo createInfo)
		{
			ulong hash = createInfo.ToHash();
			IShader? shader = FindByHash(hash);
			if(shader != null) return shader;

			pShaders[hash] = shader = CreateShader(createInfo);
			pLogger.Debug($"Created Shader #{hash.ToString("X16")}");
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
