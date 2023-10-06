using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class ObjectWrapper : Diligent.IObject
	{
		public IDisposable Handler { get; private set; } 
		public ObjectWrapper(IDisposable handler)
		{
			Handler = handler;
		}

		public void Dispose()
		{
			Handler?.Dispose();
		}

		public T Get<T>()
		{
			T?result = (T)Handler;

			if (result == null)
				throw new InvalidCastException($"Invalid cast for type {typeof(T).Name}");
			return result;
		}

		public static ObjectWrapper Unwrap(Diligent.IObject obj)
		{
			var wrapper = obj as ObjectWrapper;
			if (wrapper is null)
				throw new InvalidCastException("Invalid Wrapper Type.");
			return wrapper;
		}
	}
}
