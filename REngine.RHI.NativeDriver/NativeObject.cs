﻿using REngine.Core;
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
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly NativeObjectReleaseCallback pDisposeCallback;

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

		public uint StrongRefs 
		{
			get
			{
				if (pHandle == IntPtr.Zero)
					return 0;
				return rengine_object_strongref_count(Handle);
			}
		}
		public uint WeakRefs 
		{
			get
			{
				if (pHandle == IntPtr.Zero)
					return 0;
				return rengine_object_weakref_count(Handle);
			}
		}
		
		public event EventHandler? OnDispose;

		public NativeObject(IntPtr handle)
		{
			pDisposeCallback = OnReleaseObject;
			pHandle = handle;
#if FULL_DEBUG
			CreatedAt = string.Intern(Environment.StackTrace);
#endif

			ObjectRegistry.Lock(this);
			rengine_object_set_release_callback(handle, Marshal.GetFunctionPointerForDelegate(pDisposeCallback));
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;

			lock (pSync)
				pDisposed = true;

			BeforeRelease();

			var refs = Math.Max(StrongRefs, WeakRefs);

			IntPtr ptr = pHandle;
			pHandle = IntPtr.Zero;


			if (refs > 0)
			{
				rengine_object_set_release_callback(ptr, IntPtr.Zero);
				rengine_object_releaseref(ptr);
			}

			if (ptr != IntPtr.Zero)
			{
				lock (pSync)
				{
					ObjectRegistry.Unlock(ptr);
					pHandle = IntPtr.Zero;
				}

				OnDispose?.Invoke(this, EventArgs.Empty);
			}

			GC.SuppressFinalize(this);
		}

		protected virtual void BeforeRelease()
		{
		}

		private void OnReleaseObject(IntPtr ptr)
		{
			if (pHandle == IntPtr.Zero)
				return;
			ObjectRegistry.Unlock(this);
			pHandle = IntPtr.Zero;

			OnDispose?.Invoke(this, EventArgs.Empty);
		}

		internal void AddRef()
		{
			if (IsDisposed || pHandle == IntPtr.Zero)
				return;
			rengine_object_addref(Handle);
		}

		protected void AssertDispose()
		{
			if (IsDisposed)
				throw new ObjectDisposedException("Native Object has been already disposed");
		}
	}
}
