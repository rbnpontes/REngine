using REngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.WindowsGtk
{
	public sealed class WindowsManager : IWindowManager
	{
		public IReadOnlyList<IWindow> Windows => throw new NotImplementedException();

		public IWindowManager CloseAllWindows()
		{
			throw new NotImplementedException();
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
