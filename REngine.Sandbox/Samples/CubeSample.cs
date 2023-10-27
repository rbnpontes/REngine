using ImGuiNET;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.SceneManagement;
using REngine.RHI;
using REngine.RPI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox.Samples
{
	[Sample("Simple Cube")]
	internal class CubeSample : ISample
	{
		private ICubeRenderFeature? pCubeFeature;

		private IRenderer? pRenderer;
		private IEngine? pEngine;

		public IWindow? Window { get; set; }

		private Vector3 pCameraPos;

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pCubeFeature);

			pCubeFeature?.Camera?.Dispose();
			pCubeFeature?.Dispose();
		}

		public void Load(IServiceProvider provider)
		{
			pCubeFeature = provider.Get<BasicFeaturesFactory>().CreateCubeFeature();
			pRenderer = provider.Get<IRenderer>().AddFeature(pCubeFeature);
			pEngine = provider.Get<IEngine>();
			pCubeFeature.Camera = provider.Get<ICameraSystem>().Build();
			pCubeFeature.Camera.Transform.Position = pCameraPos = new Vector3(0f, 0f, 5.0f);
		}

		public void Update(IServiceProvider provider)
		{
			if (pCubeFeature is null)
				return;

			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0f) * 0.001f;

			pCubeFeature.Transform.EulerAngles = new Vector3(0.0f, elapsedTime, 0.0f);
			if(pCubeFeature.Camera != null)
			{
				pCubeFeature.Camera.Transform.Position = pCameraPos;
			}
		}
	}
}
