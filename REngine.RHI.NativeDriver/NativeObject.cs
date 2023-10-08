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
	internal class NativeObject : INativeObject
	{
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_releaseref(IntPtr obj);
		[DllImport(Constants.Lib)]
		public static extern void rengine_object_set_release_callback(IntPtr obj, IntPtr releaseCallback);
		[DllImport(Constants.Lib)]
		public static extern IntPtr rengine_object_getname(IntPtr obj);

		static readonly NativeObjectReleaseCallback s_disposeCallback = OnRelease;

		private readonly object pSync = new();

		private IntPtr pHandle;
		private bool pDisposed;

		public IntPtr Handle 
		{
			get
			{
				IntPtr result;
				lock (pSync)
					result = pHandle;

				return result;
			}
		}

		public bool IsDisposed
		{
			get
			{
				bool disposed = false;
				lock (pSync)
					disposed = pDisposed;
				return disposed;
			}
		}

		public event EventHandler? OnDispose;

		public NativeObject(IntPtr handle)
		{
			pHandle = handle;
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
			if (IsDisposed)
				return;

			lock(pSync)
				pDisposed = true;

			BeforeRelease();

			if (disposing)
				rengine_object_releaseref(pHandle);

			lock (pSync)
			{
				ObjectRegistry.Unlock(this);
				pHandle = IntPtr.Zero;
			}

			OnDispose?.Invoke(this, EventArgs.Empty);
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
