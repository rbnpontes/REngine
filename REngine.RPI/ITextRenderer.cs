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

	public interface ITextRenderer
	{
		public ITextRenderer SetFont(Font font);
		public ITextRenderer ClearFonts();
		public ITextRenderer RemoveFont(string fontName);
		public TextRendererBatch CreateBatch(string fontName);
	}
}
