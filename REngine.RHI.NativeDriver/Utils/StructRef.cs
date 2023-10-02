using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver.Utils
{
	internal unsafe class StructRef<T> : IDisposable where T : struct
	{
		public IntPtr Handle { get; private set; }

		public StructRef(ref T value)
		{
			int size = Unsafe.SizeOf<T>();
			void* ptr = NativeMemory.Alloc((nuint)size);
			Buffer.MemoryCopy(Unsafe.AsPointer(ref value), ptr, size, size);
			Handle = new IntPtr(ptr);
		}

		public void Dispose()
		{
			if (Handle == IntPtr.Zero)
				return;
			NativeMemory.Free(Handle.ToPointer());
			Handle = IntPtr.Zero;
		}
	}
}
