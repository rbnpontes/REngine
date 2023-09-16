using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public interface IInput
	{
		/// <summary>
		/// Listen events from Window and update input system
		/// </summary>
		/// <param name="window"></param>
		/// <returns>self instance</returns>
		public IInput BindWindow(IWindow window);

		public bool GetKeyDown(InputKey key);
		public bool GetKeyPress(InputKey key);
		public bool GetMouseDown(MouseKey key);
		public bool GetMousePress(MouseKey key);
	}
}
