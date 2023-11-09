using REngine.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public class GraphicsSettings
	{
		public uint DefaultSwapChainBufferCount { get; set; } = 3;

		public TextureFormat DefaultColorFormat { get; set; } = TextureFormat.RGBA8UNormSRGB;
		public TextureFormat DefaultDepthFormat { get; set; } = TextureFormat.D16UNorm;

		public byte DefaultTextureAnisotropy { get; set; } = 4;
		public TextureFilterMode DefaultTextureFilterMode { get; set; } = TextureFilterMode.Trilinear;

		public static GraphicsSettings FromStream(Stream stream)
		{
			GraphicsSettings? settings;
			using(TextReader reader = new StreamReader(stream))
				settings = reader.ReadToEnd().FromJson<GraphicsSettings>();
			return settings ?? new GraphicsSettings();
		}
	}
}
