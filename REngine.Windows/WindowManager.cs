using GLFW;
using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace REngine.Windows
{
	public sealed class WindowManager : IWindowManager
	{
		private readonly IExecutionPipeline pPipeline;
		private readonly ILogger<IWindowManager> pLogger;
		private readonly EngineEvents pEngineEvents;
		private readonly IEngine pEngine;

		private readonly List<WindowImpl> pWindows = new();
		private bool pDisposed = false;

		public IReadOnlyList<IWindow> Windows => throw new NotImplementedException();
		public Vector2 VideoScale { get; private set; }

		public WindowManager(
			ILogger<IWindowManager> logger,
			IExecutionPipeline pipeline,
			EngineEvents engineEvents,
			IEngine engine
		) 
		{
			pPipeline = pipeline;
			pLogger = logger;
			pEngineEvents = engineEvents;
			pEngine = engine;

			Glfw.WindowHint(Hint.ClientApi, ClientApi.None);
			Glfw.Init();
			Glfw.SetErrorCallback(HandleGlfwError);

			pEngineEvents.OnBeforeStart += HandleBeforeStart;
			pEngineEvents.OnStart += HandleEngineStart;
			pEngineEvents.OnStop += HandleEngineStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			pWindows.ForEach(x => x.Dispose());
			pWindows.Clear();

			Glfw.Terminate();
			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void HandleBeforeStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnBeforeStart -= HandleBeforeStart;

			var monitor = Glfw.Monitors.FirstOrDefault();
			VideoScale = new Vector2(monitor.ContentScale.X, monitor.ContentScale.Y);
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;
			pPipeline.AddEvent(DefaultEvents.WindowsUpdateId, (_) => Update());
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

			Glfw.PollEvents();

			int closedWindows = 0;
			foreach(var wnd in pWindows)
			{
				if (wnd.IsClosed)
					++closedWindows;
				else
					wnd.Update();
			}

			if(closedWindows == pWindows.Count)
			{
				pEngine.Stop();
				return;
			}
		}

		public IWindowManager CloseAllWindows()
		{
			if (pDisposed)
				return this;

			foreach(var wnd in pWindows)
				wnd.Close();
			return this;
		}

		public IWindow Create(WindowCreationInfo createInfo)
		{
			AssertDispose();

			if (createInfo.WindowInstance != null)
				pLogger.Warning("WindowInstance is not used by GLFW windowm manager");
			var wnd = Glfw.CreateWindow(createInfo.Size.Width, createInfo.Size.Height, createInfo.Title, GLFW.Monitor.None, GLFW.Window.None);		
			var output = new WindowImpl(wnd, createInfo.Title);
			output.Show();
			pWindows.Add(output);

			return output;
		}

		private void HandleGlfwError(ErrorCode code, IntPtr messagePtr)
		{
			string msg = Marshal.PtrToStringAnsi(messagePtr) ?? "Unknow Glfw Error";
			throw new Exception($"{msg}. Error Code: {code}");
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException("Window Manager has been disposed");
		}
	}
}
