using System.Text;
using Android.Util;
using Android.Views;
using REngine.Android.Windows;
using REngine.Core;
using REngine.Core.Android;
using REngine.Core.Resources;
using WindowManager = REngine.Android.Windows.WindowManager;

namespace REngine.Android.Sandbox
{
	[Activity(Label = "@string/app_name", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private readonly SurfaceCallback pCallback = new();
		private SandboxApp? pApp;
		private SurfaceView? pSurfaceView;
		private Task? pTask;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			if(ApplicationContext?.Assets != null)
				AndroidAssetManager.PlatformAssetManager = ApplicationContext.Assets;
			pSurfaceView = FindViewById<SurfaceView>(Resource.Id.gameView);
			pApp = new SandboxApp();
			pApp.CreateWindowAction = OnCreateWindow;
		}

		private IWindow OnCreateWindow(WindowManager wndMgr)
		{
			if (pSurfaceView is null)
				throw new NullReferenceException("Required SurfaceView");
			var surfaceView = pSurfaceView;
			return wndMgr.Create(surfaceView, pCallback);
		}

		protected override void OnStart()
		{
			base.OnStart();
			if (pApp is null || pSurfaceView is null)
				return;

			pCallback.OnCreateSurface = StartEngine;
			pSurfaceView.Holder?.AddCallback(pCallback);
		}

		private void StartEngine()
		{
			if (pCallback.NativeWindow == IntPtr.Zero)
			{
				Toast.MakeText(this.BaseContext, "Failed to Acquire NativeWindow from SurfaceView", ToastLength.Long);
				return;
			}

			pTask = Task.Factory.StartNew(EngineBootstrap, TaskCreationOptions.LongRunning);
		}

		private void EngineBootstrap()
		{
			try
			{
				EngineApplication
					.CreateStartup(pApp)
					.Setup()
					.Start()
					.Run();
			}
			catch (Exception e)
			{
				var err = new StringBuilder();
				err.Append($"Error Type: {e.GetType().Name}");
				err.AppendLine($"Message: {e.Message}");
				err.AppendLine($"StackTrace: {e.StackTrace}");
				
				var depthCount = 1;
				var innerException = e.InnerException;
				while (innerException != null)
				{
					var spacemenChars = new char[depthCount];
					for (var i = 0; i < depthCount; ++i)
						spacemenChars[i] = '\t';
					err.Append(spacemenChars);
					err.AppendLine("------- Inner Exception -------");
					err.Append(spacemenChars);
					err.AppendLine($"Error Type: {innerException.GetType().Name}");
					err.Append(spacemenChars);
					err.AppendLine($"Message: {innerException.Message}");
					err.Append(spacemenChars);
					err.AppendLine("StackTrace:");

					var stackTrace = (innerException.StackTrace ?? string.Empty).Split('\n');
					foreach (var stackTraceLine in stackTrace)
					{
						err.Append(spacemenChars);
						err.AppendLine(stackTraceLine);
					}

					innerException = innerException.InnerException;
					++depthCount;
				}

				Log.Error(nameof(MainActivity), err.ToString());
				RunOnUiThread(() => throw e);
			}
		}
	}
}