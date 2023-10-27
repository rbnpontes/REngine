using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	internal class CameraImpl : ICamera
	{
		private readonly CameraSystem pSystem;

		private bool pDisposed;
		private bool pDirty;
		private bool pDirtyProj;

		private float pNearClip = 0.01f;
		private float pFarClip = 1000f;
		private float pFov = 45f;
		private float pAspectRatio = 1;
		private bool pAutoAspectRatio = true;

		private Matrix4x4 pView = Matrix4x4.Identity;
		private Matrix4x4 pProj = Matrix4x4.Identity;

		public uint Id { get; internal set; }

		public CameraData Data
		{
			get => new CameraData
			{
				View = View,
				ViewInverse = ViewInverse,
				ViewProjection = ViewProjection,
				Position = Transform.Position.ToVector4(),
				NearClip = NearClip,
				FarClip = FarClip
			};
		}

		public Matrix4x4 View 
		{ 
			get
			{
				UpdateMatrices();
				return pView;
			}
		}

		public Matrix4x4 ViewInverse 
		{
			get => Transform.TransformMatrix;
		}

		public Matrix4x4 ViewProjection 
		{ 
			get
			{
				UpdateMatrices();
				return pProj;
			}
		}

		public float NearClip 
		{ 
			get => pNearClip; 
			set
			{
				if (pNearClip != value)
					pDirtyProj = true;
				pNearClip = value;
			}
		}

		public float FarClip 
		{ 
			get => pFarClip;
			set 
			{ 
				if(pFarClip != value)
					pDirtyProj = true;
				pFarClip = value;
			} 
		}
		public float FoV
		{
			get => pFov;
			set
			{
				if(pFov != value)
					pDirtyProj = true;
				pFov = value;
			}
		}

		public float AspectRatio
		{
			get => pAspectRatio;
			set
			{
				if(pAspectRatio != value)
				{
					pDirtyProj = true;
					pAutoAspectRatio = false; // Disable Auto Aspect Ratio
				}

				pAspectRatio = value;
			}
		}
		
		public bool AutoAspectRatio
		{
			get => pAutoAspectRatio;
			set => pAutoAspectRatio = value;
		}

		public Transform Transform { get; private set; }

		public CameraImpl(
			CameraSystem cameraSystem
		)
		{
			pSystem = cameraSystem;

			Transform = new Transform();
			Transform.OnMove += HandleTransformChanges;
			Transform.OnRotate += HandleTransformChanges;
			Transform.OnScale += HandleTransformScale;
		}

		private void HandleTransformChanges(object? sender, EventArgs e)
		{
			pDirty = true;
		}

		private void HandleTransformScale(object? sender, EventArgs e)
		{
			Transform.Scale = Vector3.One;
		}

		private void UpdateMatrices()
		{
			if (pDirty)
			{
				Matrix4x4.Invert(Transform.TransformMatrix, out pView);
				pDirty = false;
			}

			if (pDirtyProj)
			{
				pProj = Matrix4x4.CreatePerspectiveFieldOfView(
					pFov,
					pAspectRatio,
					pNearClip,
					pFarClip
				);
				pDirtyProj = false;
			}
		}

		public void Update(IWindow window)
		{
			if (!pAutoAspectRatio)
				return;
			float aspectRatio = window.Bounds.Width / (float)window.Bounds.Height;
			if (aspectRatio != pAspectRatio)
				pDirtyProj = true;
			pAspectRatio = aspectRatio;
		}

		public void Dispose()
		{
			if(pDisposed) 
				return;
			pSystem.RemoveCamera(Id);

			Transform.OnMove -= HandleTransformChanges;
			Transform.OnRotate -= HandleTransformChanges;
			Transform.OnScale -= HandleTransformScale;

			pDisposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
