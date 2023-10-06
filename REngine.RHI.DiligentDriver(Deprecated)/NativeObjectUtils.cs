using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal static class NativeObjectUtils
	{
		public static T Get<T>(object? obj)
		{
			if (obj == null)
				throw new NullReferenceException("Can´t get native reference, object is null.");
			if(obj is INativeObject)
			{
				var nativeObj = (INativeObject)obj;
				obj = nativeObj.Handle;
			}

			if (obj is null)
				throw new NullReferenceException("Can´t get native reference, native handle has been disposed.");

			return (T)obj;
		}
	}
}
