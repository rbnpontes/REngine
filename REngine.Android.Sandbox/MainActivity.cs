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
				if (e.InnerException != null)
				{
					var innerException = e.InnerException;
					err.AppendLine("------- Inner Exception -------");
					err.AppendLine($"Error Type: {innerException.GetType().Name}");
					err.AppendLine($"Message: {innerException.Message}");
					err.AppendLine($"StackTrace: {innerException.StackTrace}");
				}

				Log.Error(nameof(MainActivity), err.ToString());
				RunOnUiThread(() => throw e);
			}
		}
	}
}