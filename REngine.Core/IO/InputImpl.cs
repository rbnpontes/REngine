using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	internal class InputImpl : IInput
	{
		private byte[] pPressKeys = new byte[(int)InputKey.OemClear];
		private byte[] pMouseKeys = new byte[(int)MouseKey.XButton2];

		public IInput BindWindow(IWindow window)
		{
			return this;
		}

		public bool GetKeyDown(InputKey key)
		{
			return pPressKeys[(int)key] != 0;
		}

		public bool GetKeyPress(InputKey key)
		{
			byte pressed = pMouseKeys[(int)key];
			pPressKeys[(int)key] = Math.Min(++pPressKeys[(int)key], (byte)2);
			return pressed == 1;
		}

		public bool GetMouseDown(MouseKey key)
		{
			return pMouseKeys[(int)key] > 0;
		}

		public bool GetMousePress(MouseKey key)
		{
			byte pressed = pMouseKeys[(int)key];
			pMouseKeys[(int)key] = Math.Min(++pMouseKeys[(int)key], (byte)2);
			return pressed == 1;
		}
	}
}
