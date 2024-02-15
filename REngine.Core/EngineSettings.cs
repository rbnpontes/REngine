using REngine.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public class EngineSettings : IMergeable<EngineSettings>
	{
#if ANDROID
		public static readonly int MaxAllowedJobs = 4;
#elif WEB
		// TODO: change this value when .NET team implements Multithreading support
		public static readonly int MaxAllowedJobs = 0;
#else
		public static readonly int MaxAllowedJobs = 10;
#endif
		public static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REngine");
		public static string LoggerPath => Path.Combine(AppDataPath, "rengine.log");
		public static string EngineSettingsPath => Path.Combine(AppDataPath, "engine.rcfgs");
		public static string RenderSettingsPath => Path.Combine(AppDataPath, "render.rcfgs");
		public static string GraphicsSettingsPath => Path.Combine(AppDataPath, "graphics.rcfgs");
		public static string DriverSettingsPath => Path.Combine(AppDataPath, "driver.rcfgs");
		public static string ShaderCachePath => Path.Combine(AppDataPath, "shader-cache");
		public static string PipelineCachePath => Path.Combine(AppDataPath, "pipeline-cache.bin");
		public static string PipelineItemsPath => Path.Combine(AppDataPath, "pipelines.rcache");
		public static string AssetManagerSettingsPath => Path.Combine(AppDataPath, "assetmgr.rcfgs");
#if RENGINE_IMGUI
		public static string ImGuiSettingsPath => Path.Combine(AppDataPath, "imgui.ini");
#endif

#if ANDROID
		public static string AssetsPath => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "../Assets");
#else
		public static string AssetsPath => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets");
#endif
		public static string AssetsShadersPath => Path.Join(AssetsPath, "Shaders");
		public static string AssetsShadersPostProcessPath => Path.Join(AssetsShadersPath, "PostProcess");

		/// <summary>
		/// Defines initial available entity slots
		/// </summary>
		public uint InitialEntityCount { get; set; } = 100;
		/// <summary>
		/// Defines how entity pool will be resized when 
		/// the same will go out of space
		/// The new entity pool length will be 'NewEntityPool = new Entity[Math.Round(InitialEntityCount * EntityExpansionRate)];'
		/// </summary>
		public float EntityExpansionRate { get; set; } = 1.25f;

		/// <summary>
		/// If engine runs fasts as can and have time left
		/// Then GC will be collected
		/// </summary>
		public double GcCollectThreshold { get; set; } = 0.008f;

		/// <summary>
		/// How much time main thread will sleep if app goes to Idle(minimized) ?
		/// </summary>
		public int IdleWaitTimeMs { get; set; } = 100;

		public int JobsThreadCount { get; set; } = -1;

		public void Merge(EngineSettings value)
		{
			InitialEntityCount = value.InitialEntityCount;
			EntityExpansionRate = value.EntityExpansionRate;

			GcCollectThreshold = value.GcCollectThreshold;

			IdleWaitTimeMs = value.IdleWaitTimeMs;

			JobsThreadCount = value.JobsThreadCount;
		}

		public static EngineSettings FromStream(Stream stream)
		{
			EngineSettings? settings;
			using(TextReader reader = new StreamReader(stream))
				settings = reader.ReadToEnd().FromJson<EngineSettings>();
			return settings ?? new EngineSettings();
		}
	}
}
