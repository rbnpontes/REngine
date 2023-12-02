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
		public const string Lib = "runtimes/win-x64/native/REngine-DiligentNativeDriver.dll";
#elif LINUX
		public const string Lib = "runtimes/linux-x64/native/libREngine-DiligentNativeDriver.so";
#elif ANDROID
		public const string Lib = "libREngine-DiligentNativeDriver.so";
#endif
	}
}
