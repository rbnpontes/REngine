using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Windows
{
	internal static class InputConverter
	{
		private static readonly MouseKey[] sMouseKeys = new MouseKey[]
		{
			MouseKey.Left,
			MouseKey.Right,
			MouseKey.Middle,
			MouseKey.XButton1,
			MouseKey.XButton2
		};

		public static MouseKey GetMouseKey(GLFW.MouseButton button)
		{
			return sMouseKeys[(int)button];
		}
	}
}
