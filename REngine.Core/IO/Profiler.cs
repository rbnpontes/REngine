using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Events;
using System.Diagnostics;
using System.Runtime.InteropServices;


#if PROFILER
using bottlenoselabs.C2CS.Runtime;
using Tracy;
using static Tracy.PInvoke;
#endif
namespace REngine.Core.IO
{
	internal class DummyProfilerScope : IDisposable
	{
		public void Dispose()
		{
		}
	}
#if PROFILER
	internal sealed class ProfilerScope(TracyCZoneCtx ctx, Profiler profiler) : IDisposable
	{
		private bool pDisposed;
		public void Dispose()
		{
			if(pDisposed || profiler.IsDisposed) 
				return;
			TracyEmitZoneEnd(ctx);
			pDisposed = true;
		}
	}
#endif

	public sealed class Profiler : IDisposable
	{
		private static Profiler? sInstance;

#if PROFILER
		private readonly Dictionary<string, (CString, CString)> pTraceAllocMap = new();
		private readonly object pSync = new();

		private CString? pFrameName;
		private bool pDisabled;
#endif
		public bool IsDisposed { get; private set; }

		public static Profiler Instance
		{
			get
			{
				sInstance ??= new Profiler ();
				return sInstance;
			}
		}

		internal Profiler()
		{
			pDisabled = Debugger.IsAttached;
#if PROFILER
			if(!pDisabled)
				TracyStartupProfiler();
#endif
		}

		public void BeginFrame(string frame)
		{
			if (IsDisposed || pDisabled)
				return;
#if PROFILER
			pFrameName ??= (CString)frame;
			TracyEmitFrameMarkStart(pFrameName.Value);
#endif
		}
		public void EndFrame()
		{
			if (IsDisposed || pDisabled)
				return;
#if PROFILER
			if (pFrameName is null)
				return;
			TracyEmitFrameMarkEnd(pFrameName.Value);
#endif
		}

		public IDisposable Begin([CallerMemberName] string funcName = "",
			ProfilerColor color = default,
			[CallerFilePath] string scriptPath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			if (IsDisposed || pDisabled)
				return new DummyProfilerScope();
#if PROFILER
			lock (pSync)
			{
				if (!pTraceAllocMap.TryGetValue(funcName, out var tuple))
				{
					var source = (CString)scriptPath;
					var func = (CString)funcName;

					tuple = (source, func);
					pTraceAllocMap.Add(funcName, tuple);
				}

				var srcLoc = TracyAllocSrcloc(
					(uint)lineNumber,
					tuple.Item1,
					(ulong)scriptPath.Length,
					tuple.Item2,
					(ulong)funcName.Length);
				var ctx = TracyEmitZoneBeginAlloc(srcLoc, 1);

				if (color != default)
					TracyEmitZoneColor(ctx, (uint)color);

				return new ProfilerScope(ctx, this);
			}
#else
			return new DummyProfilerScope();
#endif
		}

		public void BeginTask(string fiberName)
		{
			if (IsDisposed || pDisabled)
				return;
#if PROFILER
			lock (pSync)
			{
				if (!pTraceAllocMap.TryGetValue(fiberName, out var tuple))
				{
					var source = (CString)fiberName;

					tuple = (source, source);
					pTraceAllocMap.Add(fiberName, tuple);
				}

				TracyFiberEnter(tuple.Item1);
			}
#endif
		}

		public void EndTask(string fiberName)
		{
			if (IsDisposed || pDisabled)
				return;
#if PROFILER
			lock (pSync)
			{
				if (!pTraceAllocMap.TryGetValue(fiberName, out var tuple))
					return;

				TracyFiberLeave(tuple.Item1);
			}
#endif
		}

		public void Dispose()
		{
			if (IsDisposed || pDisabled)
				return;
#if PROFILER
			lock (pSync)
			{
				pFrameName?.Dispose();
				pTraceAllocMap.Clear();
			}
			TracyShutdownProfiler();
#endif

			sInstance = null;
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
