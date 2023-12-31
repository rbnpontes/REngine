using REngine.Core;
using REngine.Core.Mathematics;
using REngine.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class RenderSettings : IMergeable<RenderSettings>
	{
		public string PipelineCacheFilename { get; set; } = "psocache.bin";
		/// <summary>
		/// Size of Object Uniform Buffer
		/// The buffer will be used to transfer transforms, lights or any other mutable values
		/// </summary>
		public uint ObjectBufferSize { get; set; } = 1024 * 2;
		public uint MaterialBufferSize { get; set; } = 16000; // 16kb, same of opengl
#if RENGINE_SPRITEBATCH
		/// <summary>
		/// Enable Sprite Batch instance Multi Thread Support
		/// If this option is enabled, all batches will be executed on Deferred Context
		/// </summary>
		public bool EnableSpriteBatchMultiThread { get; set; } = false;
		/// <summary>
		/// Requires <see cref="EnableSpriteBatchMultiThread"/> enabled
		/// All instance batches will be split by Jobs
		/// But to reduce GPU pressure, engine will divide instance jobs into low
		/// subset of batches.
		/// Example: If you send 1000 instance, jobs is 4 and subset is 10
		/// then engine will divide 1000 by 4 = 250 batches per job
		/// then a job will execute 25 times because at each subset, engine
		/// will execute 10 instances until reach total of instances per job
		/// </summary>
		public uint SpriteBatchInstanceSubset { get; set; } = 10;
		/// <summary>
		/// Requires <see cref="EnableSpriteBatchMultiThread"/> enabled
		/// If multi thread option is enabled, then each sprite will be execute by
		/// specified amount of jobs.
		/// If job count will be clamped to <see cref="EngineSettings.JobsThreadCount"/> and 1
		/// </summary>
		public uint SpriteBatchInstanceJobs { get; set; } = 4;
		public uint SpriteBatchInitialSize { get; set; } = 8;
		public uint SpriteBatchTextsInitialSize { get; set; } = 1;
		public uint SpriteBatchInitialInstanceSize { get; set; } = 2;
		/// <summary>
		/// When batches goes to resize, the above calc will be applied on new Array length
		/// NewLength = OldLength + (SpriteBatchInitialSize * SpriteBatchExpansionRatio)
		/// </summary>
		public float SpriteBatchExpansionRatio { get; set; } = 1.5f;
		/// <summary>
		/// When batches goes to resize, the above call will be applied on new Array length
		/// NewLength = OldLength + (SpriteBatchInitialInstanceSize * SpriteBatchInstanceExpansionRatio)
		/// </summary>
		public float SpriteBatchInstanceExpansionRatio { get; set; } = 2;
		/// <summary>
		/// Max used textures on SpriteBatch. SpriteBatch uses a TextureArray while is rendering
		/// </summary>
		public uint SpriteBatchMaxTextures { get; set; } = 8;
		/// <summary>
		/// SpriteBatch builds texture on the fly in a lazy mode
		/// this means that if you set image on the fly, SpriteBatch will create
		/// tasks on the fly, but all tasks will be wait along the frames according the value above
		/// If you use 2(2ms), sprite batch will process textures with at least 2 ms
		/// If tasks exceeds this time, then task will be waited in the next frame until all
		/// tasks finishes.
		/// </summary>
		public uint SpriteBatchTexturesBuildTimeMs { get; set; } = 2;
#endif
#if RENGINE_IMGUI
		public float ImGuiUpdateRate { get; set; } = 33.0f; // Update ImGui at 30FPS
#endif

		public void Merge(RenderSettings settings)
		{
			PipelineCacheFilename = settings.PipelineCacheFilename;
			ObjectBufferSize = settings.ObjectBufferSize;
#if RENGINE_SPRITEBATCH
			EnableSpriteBatchMultiThread = settings.EnableSpriteBatchMultiThread;
			SpriteBatchInstanceSubset = settings.SpriteBatchInstanceSubset;
			SpriteBatchInstanceJobs = settings.SpriteBatchInstanceJobs;
			
			SpriteBatchTextsInitialSize = settings.SpriteBatchTextsInitialSize;
			SpriteBatchInitialSize = settings.SpriteBatchInitialSize;
			SpriteBatchInitialInstanceSize = settings.SpriteBatchInitialInstanceSize;
			SpriteBatchExpansionRatio = settings.SpriteBatchExpansionRatio;
			SpriteBatchInstanceExpansionRatio = settings.SpriteBatchInstanceExpansionRatio;
			SpriteBatchMaxTextures = settings.SpriteBatchMaxTextures;
			SpriteBatchTexturesBuildTimeMs = settings.SpriteBatchTexturesBuildTimeMs;
#endif
#if RENGINE_IMGUI
			ImGuiUpdateRate = settings.ImGuiUpdateRate;
#endif

			MaterialBufferSize = settings.MaterialBufferSize; 
		}

		public static RenderSettings FromStream(Stream stream)
		{
			RenderSettings? settings;
			using(TextReader reader = new StreamReader(stream))
				settings = reader.ReadToEnd().FromJson<RenderSettings>();
			return settings ?? new RenderSettings();
		}
	}
}
