using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
	internal sealed class ProfilerScope(TracyCZoneCtx ctx) : IDisposable
	{
		private bool pDisposed;
		public void Dispose()
		{
			if(pDisposed) 
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
#endif
		public static Profiler Instance
		{
			get
			{
				sInstance ??= new Profiler ();
				return sInstance;
			}
		}

		public void BeginFrame(string frame)
		{
#if PROFILER
			pFrameName ??= (CString)frame;
			TracyEmitFrameMarkStart(pFrameName.Value);
#endif
		}
		public void EndFrame()
		{
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

				return new ProfilerScope(ctx);
			}
#else
			return new DummyProfilerScope();
#endif
		}

		public IDisposable BeginTask([CallerMemberName] string funcName = "",
			ProfilerColor color = default,
			[CallerFilePath] string scriptPath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
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
				var ctx = TracyEmitFi
			}
#else
			return new DummyProfilerScope();
#endif
		}

		public void Dispose()
		{
			sInstance = null;
#if PROFILER
			lock (pSync)
			{
				pFrameName?.Dispose();
				foreach (var kvp in pTraceAllocMap.Values)
				{
					kvp.Item1.Dispose();
					kvp.Item2.Dispose();
				}
				pTraceAllocMap.Clear();
			}
#endif
			GC.SuppressFinalize(this);
		}
	}
}
