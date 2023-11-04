using ImGuiNET;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.SceneManagement;
using REngine.Core.WorldManagement;
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

		private Transform? pObjectTransform;

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

			var entityMgr = provider.Get<EntityManager>();
			var transformSys = provider.Get<TransformSystem>();

			Transform cameraTransform = transformSys.CreateTransform();
			Transform objectTransform = transformSys.CreateTransform();

			Camera camera = provider.Get<CameraSystem>().CreateCamera();

			Entity cameraEntity = entityMgr.CreateEntity("Main Camera");
			Entity objectEntity = entityMgr.CreateEntity("Cube Object");

			cameraEntity.AddComponent(cameraTransform).AddComponent(camera);
			objectEntity.AddComponent(objectTransform);

			pCubeFeature.Camera = camera;
			pCubeFeature.Transform = objectTransform;
			cameraTransform.Position = new Vector3(0f, 0f, 5.0f);

			pObjectTransform = objectTransform;
		}

		public void Update(IServiceProvider provider)
		{
			if (pCubeFeature is null || pObjectTransform is null)
				return;

			float elapsedTime = (float)(pEngine?.ElapsedTime ?? 0.0f) * 0.001f;

			pObjectTransform.EulerAngles = new Vector3(0.0f, elapsedTime, 0.0f);
		}
	}
}
