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
		public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REngine");
		public static readonly string LoggerPath = Path.Combine(AppDataPath, "rengine.log");
		public static readonly string EngineSettingsPath = Path.Combine(AppDataPath, "engine.rcfgs");
		public static readonly string RenderSettingsPath = Path.Combine(AppDataPath, "render.rcfgs");
		public static readonly string GraphicsSettingsPath = Path.Combine(AppDataPath, "graphics.rcfgs");
		public static readonly string DriverSettingsPath = Path.Combine(AppDataPath, "driver.rcfgs");
		public static readonly string ShaderCachePath = Path.Combine(AppDataPath, "shader-cache");
		public static readonly string PipelineCachePath = Path.Combine(AppDataPath, "pipeline-cache.bin");
		public static readonly string PipelineItemsPath = Path.Combine(AppDataPath, "pipelines.rcache");

		public static readonly string AssetsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets");
		public static readonly string AssetsShadersPath = Path.Join(AssetsPath, "Shaders");
		public static readonly string AssetsShadersPostProcessPath = Path.Join(AssetsShadersPath, "PostProcess");
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

		public void Merge(EngineSettings value)
		{
			InitialEntityCount = value.InitialEntityCount;
			EntityExpansionRate = value.EntityExpansionRate;
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
