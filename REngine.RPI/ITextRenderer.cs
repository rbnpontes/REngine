using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class TextRendererException : Exception
	{
		public TextRendererException(string message) : base(message) { }
	}

	public abstract class TextRendererBatch : IDisposable
	{
		public abstract IPipelineState PipelineState { get; }
		public abstract IBuffer VertexBuffer { get; }
		public abstract ITexture FontTexture { get; }
		public abstract uint NumVertices { get; }

		public abstract void Dispose();
	}

	public struct TextRendererCreateInfo
	{
		public string Text;
		public Color Color;
		public Vector2 Position;
		public Font Font;
	}

	public interface ITextRenderer
	{
		/// <summary>
		/// Get Internal Stored GPU Texture atlas
		/// TextRenderer stores texture by font name
		/// If you use a Font with same name of previous registered
		/// you will get an invalid texture
		/// </summary>
		/// <param name="font">Font object</param>
		/// <returns>GPU Texture Atlas</returns>
		public ITexture? GetFontTexture(Font font);
		public TextRendererBatch CreateBatch(TextRendererCreateInfo createInfo);
	}
}
