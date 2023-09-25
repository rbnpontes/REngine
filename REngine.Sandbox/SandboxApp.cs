using Diligent;
using Microsoft.VisualBasic.Devices;
using REngine.Assets;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
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
		private SampleForm? pSampleForm;
		public SandboxApp() : base(typeof(SandboxApp))
		{
		}

		protected override IWindow OnSetupWindow(IWindowManager windowManager)
		{
			pSampleForm = new SampleForm();
			pSampleForm.Show();
			var wnd = windowManager.Create(new WindowCreationInfo
			{
				Control = pSampleForm.GameContent
			});
			pSampleForm.GameWindow = wnd;
			return wnd;
		}
		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);
			pSampleForm?.EngineStart(provider);
		}

		public override void OnUpdate(IServiceProvider provider)
		{
			pSampleForm?.CurrentSample?.Update(provider);
		}
	}
}
