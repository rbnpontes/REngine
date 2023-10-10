using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	internal class InputConverter
	{
		private static readonly MouseKey[] MouseKeys = new MouseKey[] 
		{
			MouseKey.None,
			MouseKey.Left,
			MouseKey.Middle,
			MouseKey.Right,
			MouseKey.None,
			MouseKey.None,
			MouseKey.None,
			MouseKey.None,
			MouseKey.XButton1,
			MouseKey.XButton2,
		};


		public static MouseKey GetMouseKey(uint button)
		{
			return MouseKeys[button];
		}
	}
}
