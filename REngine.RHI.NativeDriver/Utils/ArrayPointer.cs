using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class ArrayPointer<T> : IDisposable
	{
		private GCHandle pGCHandle;
		public IntPtr Handle { get; private set; }
		public ArrayPointer(T[] values)
		{
			pGCHandle = GCHandle.Alloc(values, GCHandleType.Pinned);
			Handle = pGCHandle.AddrOfPinnedObject();
		}

		public void Dispose()
		{
			if (Handle == IntPtr.Zero)
				return;

			pGCHandle.Free();
			Handle = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}
}
