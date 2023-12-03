using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Resources
{
	public abstract class Font
	{
		public const string CharMap = "ABCDEFGHIJKLMNOPQRSTUVXYWZÇabcdefghijklmnopqrstuvxywzéèãç1234567890-=_'\"/\\*+,.:;~][´`^|<>?!@#$%¨&(){}ºª§ ";

		public abstract string Name { get; }
		public abstract Image Atlas { get; }
		public abstract Size AtlasSize { get; }
		public abstract Size CharSize { get; }

		public abstract byte GetGlyphIndex(int charCode);
		public abstract Point GetOffset(byte glyphIndex);
		public abstract Rectangle GetBounds(byte glyphIndex);
		public abstract Point GetAdvance(byte glyphIndex);

		/// <summary>
		/// Create an Optimized Font. Use when you need to reduce memory usage
		/// This optimized font will not contain atlas image.
		/// </summary>
		/// <returns></returns>
		public abstract Font Optimize();
	}
}
