using System.Runtime.InteropServices;

namespace REngine.RHI.NativeDriver.Utils
{
	internal class ArrayPointer<T> : IDisposable
	{
		private GCHandle pGCHandle;
		public IntPtr Handle { get; private set; }
		public ArrayPointer(T[] values)
		{
			if (values.Length == 0)
			{
				Handle = IntPtr.Zero;
				return;
			}
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
