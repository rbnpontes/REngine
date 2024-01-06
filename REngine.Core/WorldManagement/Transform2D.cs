using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;

namespace REngine.Core.WorldManagement
{
	public sealed class Transform2DSerializer : ComponentSerializer<Transform2D>
	{
		private struct SerializeData
		{
			public int RefId;
			public Vector2 Position;
			public int ZIndex;
			public float Rotation;
			public Vector2 Scale;
			public int ParentId;
		}

		private readonly Transform2DSystem pSystem;
		public Transform2DSerializer(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			pSystem = serviceProvider.Get<Transform2DSystem>();
		}

		public override Type GetSerializeType()
		{
			return typeof(SerializeData);
		}

		public override Component Create()
		{
			return pSystem.CreateTransform();
		}

		public override object OnSerialize(Component component)
		{
			if (component is not Transform2D transform)
				throw new InvalidCastException($"Expected '{nameof(Transform2D)}' type.");

			return new SerializeData
			{
				RefId = component.GetHashCode(),
				Position = transform.Position,
				ZIndex = transform.ZIndex,
				Rotation = transform.Rotation,
				Scale = transform.Scale,
				ParentId = transform.Parent?.GetHashCode() ?? 0
			};
		}

		private readonly Dictionary<int, Transform2D> pTransformLookup = new();
		private readonly Queue<SerializeData> pUnresolvedParents = new();
		public override Component OnDeserialize(object componentData)
		{
			var data = (SerializeData)componentData;
			var transform = pSystem.CreateTransform();
			transform.Position = data.Position;
			transform.ZIndex = data.ZIndex;
			transform.Rotation = data.Rotation;
			transform.Scale = data.Scale;
			pTransformLookup[data.RefId] = transform;
			pUnresolvedParents.Enqueue(data);
			return transform;
		}

		public override void OnAfterDeserialize()
		{
			while (pUnresolvedParents.TryDequeue(out var data))
			{
				if(data.ParentId == 0)
					continue;

				var transform = pTransformLookup[data.RefId];
				var parent = pTransformLookup[data.ParentId];
				parent.AddChild(transform);
			}
			pTransformLookup.Clear();
		}
	}

	[ComponentSerializer(typeof(Transform2DSerializer))]
	public sealed class Transform2D : Component
	{
		private readonly Transform2DSystem pSystem;
		public int Id { get; private set; }

		public Vector2 Position
		{
			get
			{
				pSystem.GetPosition(this, out var value);
				return value;
			}
			set => pSystem.SetPosition(this, value);
		}

		public int ZIndex
		{
			get
			{
				ValidateDispose();
				pSystem.GetZIndex(this, out var value);
				return value;
			}
			set => pSystem.SetZIndex(this, value);
		}

		public int WorldZIndex
		{
			get
			{
				ValidateDispose();
				pSystem.GetWorldZIndex(Id, out var value);
				return value;
			}
		}

		public float Rotation
		{
			get
			{
				pSystem.GetRotation(this, out var rotation);
				return rotation;
			}
			set => pSystem.SetRotation(this, value);
		}

		public Vector2 Scale
		{
			get
			{
				pSystem.GetScale(this, out var scale);
				return scale;
			}
			set => pSystem.SetScale(this, value);
		}

		public Vector2 WorldPosition
		{
			get
			{
				pSystem.GetWorldPosition(this, out var pos);
				return pos;
			}
		}

		public float WorldRotation
		{
			get
			{
				pSystem.GetWorldRotation(this, out var value);
				return value;
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
				pSystem.GetWorldTransform(this, out var matrix);
				return matrix;
			}
		}

		public Transform2D? Parent => pSystem.GetParent(this);

		public IEnumerable<Transform2D> Children => pSystem.GetChildren(this);

		public RectangleF Bounds
		{
			get
			{
				pSystem.GetBounds(this, out var bounds);
				return bounds;
			}
		}

		public Vector2 Up => Vector2.Transform(new Vector2(0, 1), TransformMatrix);
		public Vector2 Down => Vector2.Transform(new Vector2(0, -1), TransformMatrix);
		public Vector2 Left => Vector2.Transform(new Vector2(-1, 0), TransformMatrix);
		public Vector2 Right => Vector2.Transform(new Vector2(1, 0), TransformMatrix);

		public Transform2D(Transform2DSystem system, int id)
		{
			pSystem = system;
			Id = id;
		}

		protected override void OnDispose()
		{
			pSystem.Destroy(this);
		}

		public void AddChild(Transform2D child)
		{
			pSystem.AddChild(this, child);
		}

		public void RemoveChild(Transform2D child)
		{
			pSystem.RemoveChild(this, child);
		}

		public void GetSnapshot(out Transform2DSnapshot snapshot)
		{
			pSystem.GetSnapshot(this, out snapshot);
		}
	}
}
