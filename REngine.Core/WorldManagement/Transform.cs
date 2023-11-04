using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public sealed class Transform : Component
	{
		private readonly TransformSystem pSystem;

		public int Id { get; private set; }

		public Transform? Parent
		{
			get => pSystem.GetParent(this);
		}

		public Vector3 Position
		{
			get
			{
				pSystem.GetPosition(this, out var position);
				return position;
			}
			set
			{
				pSystem.SetPosition(this, value);
			}
		}

		public Quaternion Rotation
		{
			get
			{
				pSystem.GetRotation(this, out var rotation);
				return rotation;
			}
			set
			{
				pSystem.SetRotation(this, value);
			}
		}

		public Vector3 EulerAngles
		{
			get
			{
				pSystem.GetEulerAngles(this, out var eulerAngles);
				return eulerAngles;
			}
			set
			{
				pSystem.SetEulerAngles(this, value);
			}
		}

		public Vector3 Scale
		{
			get
			{
				pSystem.GetScale(this, out var scale);
				return scale;
			}
			set
			{
				pSystem.SetScale(this, value);
			}
		}

		public Matrix4x4 TransformMatrix
		{
			get
			{
				pSystem.GetTransformMatrix(this, out var matrix);
				return matrix;
			}
		}

		public Matrix4x4 WorldTransformMatrix
		{
			get
			{
				pSystem.GetWorldTransformMatrix(this, out var matrix);
				return matrix;
			}
		}

		public Vector3 Forward
		{
			get => Vector3.Transform(new Vector3(0, 0, +1), Rotation);
		}
		public Vector3 Backward
		{
			get => Vector3.Transform(new Vector3(0, 0, -1), Rotation);
		}
		public Vector3 Up
		{
			get => Vector3.Transform(new Vector3(0, +1, 0), Rotation);
		}
		public Vector3 Down
		{
			get => Vector3.Transform(new Vector3(0, -1, 0), Rotation);
		}
		public Vector3 Left
		{
			get => Vector3.Transform(new Vector3(-1, 0, 0), Rotation);
		}
		public Vector3 Right
		{
			get => Vector3.Transform(new Vector3(+1, 0, 0), Rotation);
		}

		internal Transform(TransformSystem transformSystem, int id)
		{
			pSystem = transformSystem;

			Id = id;
		}

		protected override void OnDispose()
		{
			pSystem.Destroy(Id);
		}
	}
}
