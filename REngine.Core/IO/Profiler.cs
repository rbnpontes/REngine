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
using Tracy.Generated;
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

		private readonly DummyProfilerScope pDummyProfilerScope = new();
#if PROFILER
		private readonly Dictionary<string, (CString, CString)> pTraceAllocMap = new();
		private readonly object pSync = new();

		private CString? pFrameName;
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

		private Profiler()
		{
#if PROFILER
			TracyStartupProfiler();
#endif
		}

		public void BeginFrame(string frame)
		{
			if (IsDisposed)
				return;
#if PROFILER
			pFrameName ??= (CString)frame;
			TracyEmitFrameMarkStart(pFrameName.Value);
#endif
		}
		public void EndFrame()
		{
			if (IsDisposed)
				return;
#if PROFILER
			if (pFrameName is null || TracyConnected() == 0)
				return;
			TracyEmitFrameMarkEnd(pFrameName.Value);
#endif
		}

		public IDisposable Begin([CallerMemberName] string funcName = "",
			ProfilerColor color = default,
			[CallerFilePath] string scriptPath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			if (IsDisposed)
				return pDummyProfilerScope;
#if PROFILER
			if (TracyConnected() == 0)
				return pDummyProfilerScope;
			
			lock (pSync)
			{
				var srcLoc = TracyProfiler.AllocSourceLocation(
					lineNumber,
					scriptPath,
					funcName);
				
				var ctx = TracyEmitZoneBeginAlloc(srcLoc, 1);
				
				if (color != default)
					TracyEmitZoneColor(ctx, (uint)color);
				
				return new ProfilerScope(ctx, this);
			}
#else
			return pDummyProfilerScope;
#endif
		}

		public void BeginTask(string fiberName)
		{
			if (IsDisposed)
				return;
#if PROFILER
			if (TracyConnected() == 0)
				return;
			
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
			if (IsDisposed)
				return;
#if PROFILER
			if (TracyConnected() == 0)
				return;
			
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
			if (IsDisposed)
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
		}
	}
}
