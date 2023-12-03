using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Media;
using Android.Util;
using Android.Views;
using REngine.Android.Windows;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RPI;
using REngine.Sandbox;
using IWindowManager = REngine.Core.IWindowManager;
using WindowManager = REngine.Android.Windows.WindowManager;

namespace REngine.Android.Sandbox
{
	internal class SandboxApp() : App(typeof(SandboxApp))
	{
		public Func<WindowManager, IWindow> CreateWindowAction { get; set; } = (_)=> throw new NotImplementedException();

		public override void OnSetupModules(List<IModule> modules)
		{
			base.OnSetupModules(modules);
			modules.Add(new WindowsModule());
		}

		protected override IWindow OnSetupWindow(IWindowManager windowManager)
		{
			if(windowManager is WindowManager wndMgr)
				return CreateWindowAction(wndMgr);
			throw new ArgumentException($"Expected {nameof(WindowManager)} instance type.");
		}


		public override void OnSetup(IServiceRegistry registry)
		{
			base.OnSetup(registry);
			registry.Add<IAssetManager, AndroidAssetManager>();
			Log.Debug(nameof(SandboxApp), "Setup");
		}

		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);
#if RENGINE_IMGUI
			IRenderer renderer = provider.Get<IRenderer>();
			IImGuiSystem imGuiSystem = provider.Get<IImGuiSystem>();

			renderer.AddFeature(imGuiSystem.Feature, 1000/*ImGui Feature must execute at last*/);
#endif

			provider.Get<RenderState>().DefaultClearColor = Color.Aqua;
			provider.Get<EngineEvents>().OnBeforeStop += OnBeforeStop;

			Log.Debug(nameof(SandboxApp), "Started");
		}

		protected override void OnSetupEngineSettings(EngineSettings engineSettings)
		{
			engineSettings.JobsThreadCount = 1;
		}

		private void OnBeforeStop(object? sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override DriverSettings OnCreateDriverSettings(IServiceProvider serviceProvider)
		{
			return new DriverSettings
			{
				Backend = GraphicsBackend.OpenGL
			};
		}
	}

}
