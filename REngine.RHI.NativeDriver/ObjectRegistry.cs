using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal static class ObjectRegistry
	{
		private static readonly Dictionary<IntPtr, WeakReference<NativeObject>> sTrackingObjects = new();

		public static void Lock(NativeObject obj)
		{
			Lock(obj, obj.Handle);
		}
		public static void Lock(NativeObject obj, IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new NullReferenceException("Can´t lock object with IntPtr.Zero handle");
			lock (sTrackingObjects)
			{
				if(sTrackingObjects.TryGetValue(handle, out WeakReference<NativeObject>? output))
				{
					output.SetTarget(obj);
					return;
				}

				sTrackingObjects.Add(handle, new WeakReference<NativeObject>(obj));
			}
		}
		public static void Unlock(NativeObject obj)
		{
			Unlock(obj.Handle);
		}
		public static void Unlock(IntPtr ptr)
		{
			lock (sTrackingObjects)
			{
				sTrackingObjects.Remove(ptr);
			}
		}

		public static void ClearRegistry()
		{
			lock (sTrackingObjects)
			{
				sTrackingObjects.Clear();
			}
		}

		public static NativeObject? Acquire(IntPtr ptr)
		{
			NativeObject? obj = null;
			lock (sTrackingObjects)
			{
				sTrackingObjects.TryGetValue(ptr, out WeakReference<NativeObject>? weakRef);
				weakRef?.TryGetTarget(out obj);
			}
			return obj;
		}
	}
}
