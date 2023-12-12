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
using REngine.Core.Desktop;
using REngine.Core.IO;

namespace REngine.Sandbox
{
	internal class SandboxApp : App
	{
		private readonly SampleWindow pSampleWindow = new();
		public override void OnStart(IServiceProvider provider)
		{
			var window = MainWindow;
			window.Title = "[REngine] Samples";
			window.Size = new Size(800, 500);
			
			provider.Get<EngineEvents>().OnBeforeStop += OnBeforeStop;
			pSampleWindow.EngineStart(provider);
		}

		private void OnBeforeStop(object? sender, EventArgs e)
		{
			pSampleWindow.EngineStop();
		}

		public override void OnUpdate(IServiceProvider provider)
		{
			pSampleWindow.EngineUpdate(provider);
		}
	}
}
