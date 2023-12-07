using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RPI;
using REngine.RPI.Features;
using REngine.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal class SandboxApp() : App(typeof(SandboxApp))
	{
		private SampleWindow? pSampleWindow;

		public override void OnSetupModules(List<IModule> modules)
		{
			base.OnSetupModules(modules);
			modules.Add(new WindowsModule());
		}

		protected override IWindow OnSetupWindow(IWindowManager windowManager)
		{
			pSampleWindow = new SampleWindow();
			return windowManager.Create(new WindowCreationInfo
			{
				Title = "[REngine] Samples",
				Size = new System.Drawing.Size(800, 500)
			});
		}
		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);

#if RENGINE_IMGUI
			IRenderer renderer = provider.Get<IRenderer>();
			IImGuiSystem imGuiSystem = provider.Get<IImGuiSystem>();

			renderer.AddFeature(imGuiSystem.Feature, 1000/*ImGui Feature must execute at last*/);
#endif
			provider.Get<EngineEvents>().OnBeforeStop += OnBeforeStop;
			pSampleWindow?.EngineStart(provider);
		}

		private void OnBeforeStop(object? sender, EventArgs e)
		{
			pSampleWindow?.EngineStop();
		}

		public override void OnUpdate(IServiceProvider provider)
		{
			pSampleWindow?.EngineUpdate(provider);
		}

		protected override DriverSettings OnCreateDriverSettings(IServiceProvider serviceProvider)
		{
			return new DriverSettings
			{
#if WINDOWS
				Backend = GraphicsBackend.OpenGL,
#else
				Backend = GraphicsBackend.Vulkan,
#endif
			};
		}
	}
}
