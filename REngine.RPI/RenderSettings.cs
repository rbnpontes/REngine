using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class RenderSettings : IMergeable<RenderSettings>
	{
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

		public uint SpriteBatchInitialSize { get; set; } = 8;

		public void Merge(RenderSettings settings)
		{
			FrameBufferSize = settings.FrameBufferSize;
			ObjectBufferSize = settings.ObjectBufferSize;
			SpriteBatchInitialSize = settings.SpriteBatchInitialSize;
		}
	}
}
