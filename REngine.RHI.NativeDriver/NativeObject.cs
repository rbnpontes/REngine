using REngine.Core;
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

		static readonly NativeObjectReleaseCallback s_disposeCallback = OnRelease;

		public IntPtr Handle { get; protected set; }

		public NativeObject(IntPtr handle)
		{
			Handle = handle;
			ObjectRegistry.Lock(this);

			rengine_object_set_release_callback(handle, Marshal.GetFunctionPointerForDelegate(s_disposeCallback));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;

			BeforeRelease();
			
			if (disposing)
			{
				rengine_object_releaseref(Handle);
			}

			ObjectRegistry.Unlock(this);
			Handle = IntPtr.Zero;
		}

		protected virtual void BeforeRelease()
		{
		}

		protected static void OnRelease(IntPtr handle)
		{
			NativeObject? obj = ObjectRegistry.Acquire(handle);
			obj?.Dispose(false);
		}
	}
}
