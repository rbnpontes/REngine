using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RPI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal class SandboxApp : App
	{
		private SampleWindow? pSampleWindow;
		public SandboxApp() : base(typeof(SandboxApp))
		{
		}

		protected override IWindow OnSetupWindow(IWindowManager windowManager)
		{
			pSampleWindow = new SampleWindow();

			return windowManager.Create(new WindowCreationInfo
			{
				WindowInstance = pSampleWindow.GameContentWindow
			});
		}
		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);

#if RENGINE_IMGUI
			IRenderer renderer = provider.Get<IRenderer>();
			IImGuiSystem imGuiSystem = provider.Get<IImGuiSystem>();

			renderer.AddFeature(imGuiSystem.Feature);
#endif
			pSampleWindow?.EngineStart(provider);
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
				Backend = GraphicsBackend.D3D11,
#else
				Backend = GraphicsBackend.Vulkan,
#endif
			};
		}
	}
}
