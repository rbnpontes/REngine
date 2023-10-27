using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public interface ICamera : IDisposable
	{
		public uint Id { get; }
		public CameraData Data { get; }
		public Matrix4x4 View { get; }
		public Matrix4x4 ViewInverse { get; }
		public Matrix4x4 ViewProjection { get; }
		public Transform Transform { get; }
		public float NearClip { get; set; }
		public float FarClip { get; set; }
		public float FoV { get; set; }
		public bool AutoAspectRatio { get; set; }
		public float AspectRatio { get; set; }
	}
}
