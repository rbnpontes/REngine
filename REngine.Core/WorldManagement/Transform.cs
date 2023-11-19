using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public sealed class TransformSerializer : ComponentSerializer<Transform>
	{
		private struct SerializeData
		{
			public int RefId;
			public Vector3 Position;
			public Quaternion Rotation;
			public Vector3 Scale;
			public int ParentId;
		}

		private readonly TransformSystem pSystem;

		public TransformSerializer(
			IServiceProvider serviceProvider,
			TransformSystem system
		) : base(serviceProvider)
		{
			pSystem = system;
		}

		public override Type GetSerializeType()
		{
			return typeof(SerializeData);
		}

		public override object OnSerialize(Component component)
		{
			if (component is not Transform transform)
				throw new InvalidCastException($"Expected '{nameof(Transform)}' type.");

			return new SerializeData
			{
				RefId = component.GetHashCode(),
				Position = transform.Position,
				Rotation = transform.Rotation,
				Scale = transform.Scale,
				ParentId = transform.Parent?.GetHashCode() ?? 0
			};
		}

		private readonly Dictionary<int, Transform> pTransformLookup = new Dictionary<int, Transform>();
		private readonly Queue<SerializeData> pUnresolvedParents = new Queue<SerializeData>();
		public override Component OnDeserialize(object componentData)
		{
			var data = (SerializeData)componentData;
			var transform = pSystem.CreateTransform();
			transform.Position = data.Position;
			transform.Rotation = data.Rotation;
			transform.Scale = data.Scale;
			pTransformLookup[data.RefId] = transform;
			pUnresolvedParents.Enqueue(data);
			return transform;
		}

		public override void OnAfterDeserialize()
		{
			// Resolve Parents
			while(pUnresolvedParents.TryDequeue(out var data))
			{
				if (data.ParentId == 0)
					continue;
				var transform = pTransformLookup[data.RefId];
				var parent = pTransformLookup[data.ParentId];
				parent.AddChild(transform);
			}
			pTransformLookup.Clear();
		}
	}

	[ComponentSerializer(typeof(TransformSerializer))]
	public sealed class Transform : Component
	{
		private readonly TransformSystem pSystem;

		public int Id { get; private set; }

		public Transform? Parent => pSystem.GetParent(this);

		public IEnumerable<Transform> Children => pSystem.GetChildren(this);

		public Vector3 Position
		{
			get
			{
				pSystem.GetPosition(this, out var position);
				return position;
			}
			set => pSystem.SetPosition(this, value);
		}

		public Quaternion Rotation
		{
			get
			{
				pSystem.GetRotation(this, out var rotation);
				return rotation;
			}
			set => pSystem.SetRotation(this, value);
		}

		public Vector3 EulerAngles
		{
			get
			{
				pSystem.GetEulerAngles(this, out var eulerAngles);
				return eulerAngles;
			}
			set => pSystem.SetEulerAngles(this, value);
		}

		public Vector3 Scale
		{
			get
			{
				pSystem.GetScale(this, out var scale);
				return scale;
			}
			set => pSystem.SetScale(this, value);
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

		public Vector3 Forward => Vector3.Transform(new Vector3(0, 0, +1), Rotation);

		public Vector3 Backward => Vector3.Transform(new Vector3(0, 0, -1), Rotation);

		public Vector3 Up => Vector3.Transform(new Vector3(0, +1, 0), Rotation);

		public Vector3 Down => Vector3.Transform(new Vector3(0, -1, 0), Rotation);

		public Vector3 Left => Vector3.Transform(new Vector3(-1, 0, 0), Rotation);

		public Vector3 Right => Vector3.Transform(new Vector3(+1, 0, 0), Rotation);

		internal Transform(TransformSystem transformSystem, int id)
		{
			pSystem = transformSystem;

			Id = id;
		}

		protected override void OnDispose()
		{
			pSystem.Destroy(Id);
		}

		public void AddChild(Transform child)
		{
			pSystem.AddChild(this, child);
		}
		public void RemoveChild(Transform child)
		{
			pSystem.RemoveChild(this, child);
		}
	}
}
