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
#if FULL_DEBUG
		private static readonly Dictionary<IntPtr, (string, string)> sDbgPtrOwners = new();
#endif

		public static void Lock(NativeObject obj)
		{
			Lock(obj, obj.Handle);
		}
#if FULL_DEBUG
		private static void ValidateUndisposedHandlers()
		{
			var owners = sTrackingObjects.Where(x => !x.Value.TryGetTarget(out var obj)).Select(x =>
			{
				if(sDbgPtrOwners.TryGetValue(x.Key, out var ownerPair))
				{
					var (name, stacktrace) = ownerPair;
					return (x.Key, name, stacktrace);
				}
				return (x.Key, "Unknow", string.Empty);
			});

			if (owners.Count() > 0)
			{
				StringBuilder message = new StringBuilder();
				message.AppendLine("Memory Leak Detected! Native Driver Object has not been disposed");
				message.AppendLine("Object reference has been destroyed by GC but Native Ptr has not Released");
				message.AppendLine("======================================");
				foreach(var owner in owners)
				{
					var (ptr, name, stacktrace) = owner;
					message.Append("Handle: ");
					message.AppendLine(ptr.ToInt64().ToString("{0:x}"));
					message.AppendLine($"TypeName: {name}");
					message.AppendLine($"Stack: {stacktrace}");
					throw new REngine.Core.Exceptions.EngineFatalException(message.ToString());
				}
			}
		}
#endif
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
#if FULL_DEBUG
				sDbgPtrOwners[handle] = (obj.GetType().FullName ?? "Unknow", obj.CreatedAt);
				ValidateUndisposedHandlers();
#endif
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
#if FULL_DEBUG
				sDbgPtrOwners.Remove(ptr);
				ValidateUndisposedHandlers();
#endif
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
