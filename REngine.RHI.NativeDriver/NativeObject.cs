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
	internal partial class NativeObject : INativeObject
	{
#if FULL_DEBUG
		public string CreatedAt { get; private set; }
#endif
		private readonly object pSync = new();

		private IntPtr pHandle;
		private bool pDisposed;
		private NativeObjectReleaseCallback pDisposeCallback;

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

		public uint StrongRefs { get => rengine_object_strongref_count(Handle); }
		public uint WeakRefs { get => rengine_object_weakref_count(Handle); }
		
		public event EventHandler? OnDispose;

		public NativeObject(IntPtr handle)
		{
			pDisposeCallback = OnReleaseObject;
			pHandle = handle;
#if FULL_DEBUG
			CreatedAt = Environment.StackTrace;
#endif

			ObjectRegistry.Lock(this);
			rengine_object_set_release_callback(handle, Marshal.GetFunctionPointerForDelegate(pDisposeCallback));
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

			var refs = Math.Max(StrongRefs, WeakRefs);
			if (disposing && refs > 0)
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

		private void OnReleaseObject(IntPtr ptr)
		{
			Dispose(false);
		}

		internal void AddRef()
		{
			if (IsDisposed)
				return;
			rengine_object_addref(Handle);
		}
	}
}
