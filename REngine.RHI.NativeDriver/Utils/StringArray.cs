using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal unsafe class StringArray : IDisposable
	{
		private readonly int pLength;

		private bool pDisposed = false;

		public IntPtr* Handle { get; private set; }

		public StringArray(string[] strings)
		{
			pLength = strings.Length;
			if (strings.Length > 0)
			{
				Handle = (IntPtr*)NativeMemory.Alloc((nuint)(Unsafe.SizeOf<IntPtr>() * strings.Length));
				for (int i = 0; i < strings.Length; i++)
				{
					Handle[i] = Marshal.StringToHGlobalAnsi(strings[i]);
				}
			}
		}

		public void Dispose()
		{
			if(pDisposed || pLength == 0)
				return;

			for(int i = 0; i < pLength; ++i)
				Marshal.FreeHGlobal(Handle[i]);

			NativeMemory.Free(Handle);
			GC.SuppressFinalize(this);
			pDisposed = true;
		}
	}
}
