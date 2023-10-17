using REngine.Core;
using REngine.Core.Mathematics;
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
		/// Size of Frame Uniform Buffer
		/// The buffer will be used to transfer mutable values like time, delta time and
		/// any other engine values.
		/// </summary>
		public uint FrameBufferSize { get; set; } = 512;
		/// <summary>
		/// Size of Object Uniform Buffer
		/// The buffer will be used to transfer transforms, lights or any other mutable values
		/// </summary>
		public uint ObjectBufferSize { get; set; } = 1024 * 2;
#if RENGINE_SPRITEBATCH
		public uint SpriteBatchInitialSize { get; set; } = 8;
		public uint SpriteBatchTextsInitialSize { get; set; } = 1;
		public uint SpriteBatchInitialInstanceSize { get; set; } = 512;
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
			FrameBufferSize = settings.FrameBufferSize;
			ObjectBufferSize = settings.ObjectBufferSize;
#if RENGINE_SPRITEBATCH
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
		}
	}
}
