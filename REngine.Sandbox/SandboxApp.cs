using Diligent;
using Microsoft.VisualBasic.Devices;
using REngine.Core;
using REngine.Core.DependencyInjection;
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
		private ICubeRenderFeature? pCubeFeature;
		private GraphicsBackend pBackend;
		private IWindow? pWindow;
		private IEngine? pEngine;
		public SandboxApp() : base(typeof(SandboxApp))
		{
		}

		public override void OnStart(IServiceProvider provider)
		{
			base.OnStart(provider);
			pCubeFeature = provider.Get<BasicFeaturesFactory>().CreateCubeFeature();
			provider.Get<IRenderer>().AddFeature(pCubeFeature);
			pBackend = provider.Get<IGraphicsDriver>().Backend;
			pWindow = provider.Get<IWindow>();
			pEngine = provider.Get<IEngine>();
		}

		public override void OnUpdate(IServiceProvider provider)
		{
			var wndSize = pWindow?.Size ?? new Size();
			var worldMatrix = Matrix4x4.CreateRotationY((float)(pEngine?.ElapsedTime ?? 0.0) / 1000.0f) * Matrix4x4.CreateRotationX(-MathF.PI * 0.1f);
			var viewMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 5.0f);
			var projMatrix = CreateFoV(MathF.PI / 4.0f, wndSize.Width / (float)wndSize.Height, 0.01f, 100.0f, pBackend == GraphicsBackend.OpenGL);
			var wvpMatrix = Matrix4x4.Transpose(worldMatrix * viewMatrix * projMatrix);

			if(pCubeFeature != null)
				pCubeFeature.Transform = wvpMatrix;
		}
		
		private Matrix4x4 CreateFoV(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, bool isOpenGL)
		{
			if (fieldOfView <= 0.0f || fieldOfView >= MathF.PI)
				throw new ArgumentOutOfRangeException(nameof(fieldOfView));

			if (nearPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

			if (farPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

			if (nearPlaneDistance >= farPlaneDistance)
				throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

			float yScale = 1.0f / MathF.Tan(fieldOfView * 0.5f);
			float xScale = yScale / aspectRatio;

			Matrix4x4 result = new()
			{
				M11 = xScale,
				M22 = yScale
			};

			if (isOpenGL)
			{
				result.M33 = (farPlaneDistance + nearPlaneDistance) / (farPlaneDistance - nearPlaneDistance);
				result.M43 = -2 * nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M34 = 1.0f;
			}
			else
			{
				result.M33 = farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M43 = -nearPlaneDistance * farPlaneDistance / (farPlaneDistance - nearPlaneDistance);
				result.M34 = 1.0f;
			}

			return result;
		}
	}
}
