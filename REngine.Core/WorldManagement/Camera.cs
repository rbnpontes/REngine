using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public struct CameraCBufferData
	{
		public Matrix4x4 View;
		public Matrix4x4 ViewInverse;
		public Matrix4x4 ViewProjection;
		public Vector4 Position;
		public float NearClip;
		public float FarClip;
		public Vector2 EmptyData;
	}
	public sealed class Camera : Component
	{
		private readonly CameraSystem pSystem;
		public int Id { get; internal set; }

		public Matrix4x4 View 
		{ 
			get
			{
				ValidateDispose();
				pSystem.GetView(Id, out var view);
				return view;
			}
		}

		public Matrix4x4 ViewInverse 
		{
			get
			{
				ValidateDispose();
				pSystem.GetViewInverse(Id, out var view);
				return view;
			}
		}

		public Matrix4x4 ViewProjection 
		{ 
			get
			{
				ValidateDispose();
				pSystem.GetProjection(Id, out var projection);
				return projection;
			}
		}

		public float NearClip 
		{ 
			get
			{
				ValidateDispose();
				pSystem.GetNearClip(Id, out var nearClip);
				return nearClip;
			}
			set
			{
				ValidateDispose();
				pSystem.SetNearClip(Id, value);
			}
		}

		public float FarClip 
		{ 
			get
			{
				ValidateDispose();
				pSystem.GetFarClip(Id, out var farClip);
				return farClip;
			}
			set 
			{ 
				ValidateDispose();
				pSystem.SetFarClip(Id, value);
			} 
		}

		public float FoV
		{
			get
			{
				ValidateDispose();
				pSystem.GetFov(Id, out var fov);
				return fov;
			}
			set
			{
				ValidateDispose();
				pSystem.SetFov(Id, value);
			}
		}
		
		public float Zoom
		{
			get
			{
				ValidateDispose();
				pSystem.GetZoom(Id, out var zoom);
				return zoom;
			}
			set
			{
				ValidateDispose();
				pSystem.SetZoom(Id, value);
			}
		}
		
		public float AspectRatio
		{
			get
			{
				ValidateDispose();
				pSystem.GetAspectRatio(Id, out var aspectRatio);
				return aspectRatio;
			}
			set
			{
				ValidateDispose();
				pSystem.SetAspectRatio(Id, value);
			}
		}
		
		public bool AutoAspectRatio
		{
			get
			{
				ValidateDispose();
				pSystem.GetAutoAspectRatio(Id, out var autoAspectRatio);
				return autoAspectRatio;
			}
			set
			{
				ValidateDispose();
				pSystem.SetAutoAspectRatio(Id, value);
			}
		}

		public Transform Transform 
		{
			get
			{
				ValidateDispose();
				return pSystem.GetTransform(Id);
			}
		}

		internal Camera(
			CameraSystem cameraSystem,
			int id
		)
		{
			pSystem = cameraSystem;
			Id = id;
		}

		protected override void OnDispose()
		{
			pSystem.Destroy(Id);
		}

		public void GetCBufferData(out CameraCBufferData output)
		{
			output = new CameraCBufferData
			{
				View = View,
				ViewInverse = ViewInverse,
				ViewProjection = ViewProjection,
				Position = Transform.Position.ToVector4(),
				NearClip = NearClip,
				FarClip = FarClip,
			};
		}
	}
}
