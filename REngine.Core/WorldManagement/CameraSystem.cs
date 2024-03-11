using REngine.Core.Collections;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public struct CameraData
	{
		public bool Dirty;
		public bool DirtyProj;

		public float NearClip;
		public float FarClip;
		public float Fov;
		public float Zoom;
		public float AspectRatio;
		public bool AutoAspectRatio;

		public Matrix4x4 View;
		public Matrix4x4 Proj;

		public Camera? Component;
		public Transform? Transform;

		public Vector3 LastPos;
		public Quaternion LastRot;

		public CameraData()
		{
			Dirty = DirtyProj = true;

			NearClip = 0.01f;
			FarClip = 100.0f;
			Fov = 90f;
			Zoom = 1;
			AspectRatio = 1;
			AutoAspectRatio = true;

			View = Proj = Matrix4x4.Identity;

			LastPos = Vector3.Zero;
			LastRot = Quaternion.Identity;

			Transform = null;
			Component = null;
		}
	}

	public sealed class CameraSystem : BaseSystem<CameraData>
	{
		private readonly object pSync = new();
		private readonly IExecutionPipeline pPipeline;
		private readonly IServiceProvider pServiceProvider;
		private readonly TransformSystem pTransformSystem;
		private readonly ILogger<CameraSystem> pLogger;

		private IWindow? pMainWindow;

		public CameraSystem(
			IExecutionPipeline pipeline,
			IServiceProvider serviceProvider,
			EngineEvents engineEvents,
			TransformSystem transformSystem,
			ILoggerFactory loggerFactory
		)
		{ 
			pPipeline = pipeline;
			pServiceProvider = serviceProvider;
			pTransformSystem = transformSystem;
			pLogger = loggerFactory.Build<CameraSystem>();

			engineEvents.OnStart.Once(HandleEngineStart);
		}

		private async Task HandleEngineStart(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			pMainWindow = pServiceProvider.Get<IWindow>();

			if(pMainWindow != null)
				pPipeline.AddEvent(DefaultEvents.SceneMgtUpdateCameras, (_) => UpdateCameras());
		}

		private void UpdateCameras()
		{
			for(int i =0; i < pData.Length; ++i)
			{
				var camera = pData[i];
				if (camera.Component is null)
					continue;
				if(camera.Component.Owner is null)
				{
					// Unset Transform if Component is not attached to entity
					camera.Transform = null;
					pData[i] = camera;
					continue;
				}

				if(camera.Transform is null)
					camera.Transform = camera.Component.Owner.GetComponent<Transform>();

				if(camera.Transform is null)
				{
					pLogger.Warning($"Camera #{i} requires Transform component on Entity. Creating Transform Component!");
					camera.Transform = pTransformSystem.CreateTransform();
					camera.Dirty = camera.DirtyProj = true;
					camera.Component.Owner.AddComponent(camera.Transform);
				}

				if (pMainWindow is null)
					camera.AutoAspectRatio = false;

				if (camera.AutoAspectRatio && pMainWindow != null)
				{
					var aspectRatio = pMainWindow.Bounds.Width / (float)pMainWindow.Bounds.Height;
					if(aspectRatio != camera.AspectRatio)
						camera.DirtyProj = true;
					camera.AspectRatio = aspectRatio;
				}

				pData[i] = camera;
			}
		}

		private void UpdateMatrices(int id)
		{
			var data = pData[id];
			var transform = data.Transform;
			if (data.Component is null || transform is null)
			{
				data.View = Matrix4x4.Identity;
				data.Proj = Matrix4x4.Identity;
				pData[id] = data;
				return;
			}

			data.Dirty |= data.LastPos != transform.Position || data.LastRot != transform.Rotation || transform.Scale != Vector3.One;

			if (data.Dirty)
			{
				transform.Scale = Vector3.One;
				Matrix4x4.Invert(transform.TransformMatrix, out data.View);
				data.Dirty = false;
			}

			if (data.DirtyProj)
			{
				data.Proj = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
					(float)Math.Clamp((1.0 / Math.Tan(data.Fov * Mathf.Degrees2Radians * 0.5)) * data.Zoom, float.Epsilon, Math.Floor(Math.PI)),
					data.AspectRatio,
					data.NearClip,
					data.FarClip
				);
				data.DirtyProj = false;
			}

			pData[id] = data;
		}

		private void ValidateComponent(int id)
		{
			ValidateId(id);
			if (pData[id].Component is null)
				throw new NullReferenceException("Invalid Id. Camera component is null");
		}

		protected override int GetExpansionSize()
		{
			return 1;
		}

		public CameraSystem Destroy(int id)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				if (data.Component == null)
					return this;

				data.Component.Detach();
				pData[id] = new CameraData();
				pAvailableIdx.Enqueue(id);
			}

			return this;
		}

		public Camera CreateCamera()
		{
			Camera camera;
			lock (pSync)
			{
				int id = Acquire();
				camera = new Camera(this, id);
				CameraData data = new();
				data.Component = camera;
				pData[id] = data;
			}

			return camera;
		}
	
		public void GetView(int id, out Matrix4x4 view)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				UpdateMatrices(id);
				view = pData[id].View;
			}
		}

		public void GetViewInverse(int id, out Matrix4x4 view)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				if (data.Transform is null)
					view = Matrix4x4.Identity;
				else
					view = data.Transform.TransformMatrix;
			}
		}
		
		public void GetProjection(int id, out Matrix4x4 projection)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				UpdateMatrices(id);
				projection = pData[id].Proj;
			}
		}
	
		public void GetNearClip(int id, out float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				value = pData[id].NearClip;
			}
		}
		public void SetNearClip(int id, float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.DirtyProj |= data.NearClip != value;
				data.NearClip = value;
				pData[id] = data;
			}
		}
		public void GetFarClip(int id, out float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				value = pData[id].FarClip;
			}
		}
		public void SetFarClip(int id, float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.DirtyProj |= data.FarClip != value;
				data.FarClip = value;
				pData[id] = data;
			}
		}
		public void GetFov(int id, out float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				value = pData[id].Fov;
			}
		}
		public void SetFov(int id, float value)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.DirtyProj |= data.Fov != value;
				pData[id] = data;
			}
		}
		public void GetZoom(int id, out float zoom)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				zoom = pData[id].Zoom;
			}
		}
		public void SetZoom(int id, float zoom)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				zoom = Math.Max(zoom, float.Epsilon);
				var data = pData[id];
				data.DirtyProj |= data.Zoom != zoom;
				data.Zoom = zoom;
				pData[id] = data;
			}
		}
		public void GetAspectRatio(int id, out float aspectRatio)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				aspectRatio = pData[id].AspectRatio;
			}
		}
		public void SetAspectRatio(int id, float aspectRatio)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				if(data.AspectRatio != aspectRatio)
				{
					data.AspectRatio = aspectRatio;
					data.AutoAspectRatio = false;
					data.DirtyProj |= true;
				}

				pData[id] = data;
			}
		}
		public void GetAutoAspectRatio(int id, out bool autoAspectRatio)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				autoAspectRatio = pData[id].AutoAspectRatio;
			}
		}
		public void SetAutoAspectRatio(int id, bool autoAspectRatio)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.AutoAspectRatio = autoAspectRatio;
				pData[id] = data;
			}
		}
	
		public Transform GetTransform(int id)
		{
			Transform? transform;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				if (data.Transform is null)
				{
					data.Transform = data.Component?.Owner?.GetComponent<Transform>();
					data.Dirty = true;
				}
				if(data.Transform is null && data.Component?.Owner != null)
				{
					data.Dirty = true;
					data.Transform = pTransformSystem.CreateTransform();
					data.Component.Owner.AddComponent(data.Transform);
				}

				transform = data.Transform;
				pData[id] = data;
			}

			if (transform is null)
				throw new NullReferenceException("Can´t find transform component. Did you attach this camera to an Entity?");
			return transform;
		}
	}
}
