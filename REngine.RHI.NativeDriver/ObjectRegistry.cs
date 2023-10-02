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
			sTrackingObjects.Add(obj.Handle, new WeakReference<NativeObject>(obj));
		}
		public static void Unlock(NativeObject obj)
		{
			sTrackingObjects.Remove(obj.Handle);
		}
		public static void Unlock(IntPtr ptr)
		{
			sTrackingObjects.Remove(ptr);
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
