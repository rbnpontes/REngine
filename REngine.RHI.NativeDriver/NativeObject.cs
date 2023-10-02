using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal delegate void NativeObjectReleaseCallback(IntPtr handler);
	internal class NativeObject : IDisposable, INativeObject
	{
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_releaseref(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_set_release_callback(IntPtr obj, IntPtr releaseCallback);

		private GCHandle pHandle;
		public IntPtr Handle { get; protected set; }

		public NativeObject(IntPtr handle)
		{
			Handle = handle;
			pHandle = GCHandle.Alloc(handle, GCHandleType.Pinned);
			NativeObjectReleaseCallback releaseCallback = OnRelease;

			rengine_object_set_release_callback(handle, Marshal.GetFunctionPointerForDelegate(releaseCallback));
		}

		public void Dispose()
		{
			if (Handle == IntPtr.Zero)
				return;
			rengine_object_releaseref(Handle);
			Handle = IntPtr.Zero;
		}

		protected virtual void OnRelease(IntPtr handle)
		{
			Handle = IntPtr.Zero;
			pHandle.Free();
		}
	}
}
