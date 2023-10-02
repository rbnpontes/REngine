using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class Constants
	{
#if WINDOWS
		public const string Lib = "runtimes/win-64/REngine-DiligentNativeDriver.dll";
#elif LINUX
		public const string Lib = "runtimes/linux/REngine-DiligentNativeDriver.a";
#endif
	}
}
