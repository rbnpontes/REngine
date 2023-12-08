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
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;

namespace REngine.Sandbox
{
	internal class SandboxApp : IEngineApplication
	{
		private readonly SampleWindow pSampleWindow = new();
		public void OnSetLogger(ILogger logger)
		{
		}

		public void OnSetupModules(List<IModule> modules)
		{
		}

		public void OnSetup(IServiceRegistry registry)
		{
		}

		public void OnStart(IServiceProvider provider)
		{
			var window = provider.Get<IWindow>();
			window.Title = "[REngine] Samples";
			window.Size = new Size(800, 500);
#if RENGINE_IMGUI
			var renderer = provider.Get<IRenderer>();
			var imGuiSystem = provider.Get<IImGuiSystem>();

			renderer.AddFeature(imGuiSystem.Feature, 1000/*ImGui Feature must execute at last*/);
#endif
			provider.Get<EngineEvents>().OnBeforeStop += OnBeforeStop;
			pSampleWindow.EngineStart(provider);
		}

		private void OnBeforeStop(object? sender, EventArgs e)
		{
			pSampleWindow.EngineStop();
		}

		public void OnUpdate(IServiceProvider provider)
		{
			pSampleWindow.EngineUpdate(provider);
		}

		public void OnExit(IServiceProvider provider)
		{
		}
	}
}
