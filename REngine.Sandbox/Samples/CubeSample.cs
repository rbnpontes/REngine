using REngine.Core;
using REngine.Core.DependencyInjection;
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

		public void Dispose()
		{
			pRenderer?.RemoveFeature(pCubeFeature);
			pCubeFeature?.Dispose();
		}

		public void Load(IServiceProvider provider)
		{
			pCubeFeature = provider.Get<BasicFeaturesFactory>().CreateCubeFeature();
			pRenderer = provider.Get<IRenderer>().AddFeature(pCubeFeature);
			pEngine = provider.Get<IEngine>();
		}

		public void Update(IServiceProvider provider)
		{
			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0f) * 0.001f;
			var wndSize = Window?.Size ?? new Size();

			var worldMatrix = Matrix4x4.CreateRotationY(elapsedTime) * Matrix4x4.CreateRotationX(-MathF.PI * 0.1f);
			var viewMatrix = Matrix4x4.CreateTranslation(0.0f, -.5f, -5.0f);
			var projMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f, wndSize.Width / (float)wndSize.Height, 0.01f, 100.0f);
			var wvpMatrix = Matrix4x4.Transpose(worldMatrix * viewMatrix * projMatrix);

			if(pCubeFeature != null)
				pCubeFeature.Transform = wvpMatrix;
		}
	}
}
