using REngine.Core.Collections;
using REngine.Core.DependencyInjection;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	internal class CameraSystem : ICameraSystem
	{
		private readonly IExecutionPipeline pPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly IServiceProvider pServiceProvider;
		private EntityPool<CameraImpl> pCameras = new();

		private IWindow? pMainWindow;

		public CameraSystem(
			IExecutionPipeline pipeline,
			IServiceProvider serviceProvider,
			EngineEvents engineEvents
		)
		{ 
			pPipeline = pipeline;
			pServiceProvider = serviceProvider;
			pEngineEvents = engineEvents;

			engineEvents.OnStart += HandleEngineStart;
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pEngineEvents.OnStart -= HandleEngineStart;

			pMainWindow = pServiceProvider.Get<IWindow>();

			if(pMainWindow != null)
				pPipeline.AddEvent(DefaultEvents.SceneMgtUpdateCameras, (_) => UpdateCameras());
		}

		public ICamera Build()
		{
			var cam = new CameraImpl(this);
			int id = pCameras.Add(cam);
			cam.Id = (uint)id;

			return cam;
		}

		public IEnumerable<ICamera> GetAllCameras()
		{
			return pCameras;
		}

		public ICamera GetCameraById(uint id)
		{
			var cam = pCameras.GetEntity((int)id);
			if (cam is null)
				throw new NullReferenceException("Not found Camera. Id is invalid");
			return cam;
		}

		private void UpdateCameras()
		{
			foreach (var cam in pCameras)
				cam.Update(pMainWindow);
		}
	}
}
