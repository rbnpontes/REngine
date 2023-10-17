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
		public const string CharMap = "ABCDEFGHIJKLMNOPQRSTUVXYWZÇabcdefghijklmnopqrstuvxywzç1234567890-=_'\"/\\*+,.;~][´`^|<>?!@#$%¨&(){}ºª§";

		public abstract string Name { get; }
		public abstract Image Atlas { get; }

		public abstract byte GetGlyhIndex(int charCode);
		public abstract Point GetOffset(byte glyphIndex);
		public abstract Rectangle GetBounds(byte glyphIndex);
		public abstract Point GetAdvance(byte glyphIndex);
	}
}
