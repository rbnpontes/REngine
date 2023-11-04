using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public struct CameraData
	{
		public Matrix4x4 View;
		public Matrix4x4 ViewInverse;
		public Matrix4x4 ViewProjection;
		public Vector4 Position;
		public float NearClip;
		public float FarClip;
		public Vector2 EmptyData; // Used to align struct
	}
}
