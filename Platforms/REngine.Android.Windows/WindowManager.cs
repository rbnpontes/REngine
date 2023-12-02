using System.Numerics;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Threading;
using Android.Views;
using IWindowManager = REngine.Core.IWindowManager;

namespace REngine.Android.Windows
{
	public sealed class WindowManager : IWindowManager
	{
		private readonly ILogger<IWindowManager> pLogger;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly List<WindowImpl> pWindows = new();

		private bool pDisposed;

		public IReadOnlyList<IWindow> Windows => pWindows;
		public Vector2 VideoScale { get; } = Vector2.Zero;

		public WindowManager(
			ILoggerFactory loggerFactory,
			IExecutionPipeline pipeline,
			EngineEvents engineEvents
		)
		{
			pLogger = loggerFactory.Build<IWindowManager>();
			pExecutionPipeline = pipeline;
			pEngineEvents = engineEvents;

			pEngineEvents.OnStart += HandleEngineStart;
			pEngineEvents.OnStop += HandleEngineStop;
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			pExecutionPipeline.AddEvent(DefaultEvents.WindowsUpdateId, (_) => Update());

			pLogger.Debug("Started");
		}
		private void HandleEngineStop(object? sender, EventArgs e)
		{
			pEngineEvents.OnStop -= HandleEngineStop;
			Dispose();
		}

		private void Update()
		{
			if (pDisposed)
				return;

			foreach (var win in pWindows)
				win.Update();
		}

		public void Dispose()
		{
			if(pDisposed) return;

			pWindows.ForEach(x => x.Dispose());
			pWindows.Clear();

			pDisposed = true;
		}
		public IWindowManager CloseAllWindows()
		{
			pWindows.Clear();
			return this;
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			throw new NotSupportedException();
		}

		public IWindow Create(SurfaceView surfaceView, SurfaceCallback callback)
		{
			var window = new WindowImpl(surfaceView, callback);
			pWindows.Add(window);
			return window;
		}
	}
}
