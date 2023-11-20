using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public struct Transform2DData
	{
		public Vector2 Position;
		public int ZIndex;
		public float Rotation;
		public Vector2 Scale;
		public Matrix4x4 CachedTransformMatrix;
		public Matrix4x4 CachedWorldTransformMatrix;
		public float WorldRotation;
		public int ParentId;
		public List<int> Children;
		public bool Dirty;
		public Transform2D? Component;

		public Transform2DData()
		{
			Position = Vector2.Zero;
			Rotation = 0f;
			ZIndex = 0;
			Scale = Vector2.One;
			
			CachedTransformMatrix = Matrix4x4.Identity;
			CachedWorldTransformMatrix = Matrix4x4.Identity;
			WorldRotation = 0;
			ParentId = -1;
			Children = new List<int>();
			Component = null;
			Dirty = true;
		}
	}

	public sealed class Transform2DSystem : BaseSystem<Transform2DData>, IEnumerable<Transform2D>
	{
		private readonly object pSync = new();
		protected override int GetExpansionSize()
		{
			return 1;
		}

		public Transform2DSystem Destroy(Transform2D transform)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				if (data.Component is null)
					return this;

				data.Component.Detach();
				if (data.ParentId != -1)
				{
					var parent = pData[data.ParentId];
					parent.Children.Remove(transform.Id);
					pData[data.ParentId] = parent;
				}

				var childList = data.Children;
				foreach (var child in childList)
				{
					var childItem = pData[child];

					childItem.ParentId = data.ParentId;
					childItem.Dirty = true;
					pData[child] = childItem;
				}

				pData[transform.Id] = new Transform2DData();
				pAvailableIdx.Enqueue(transform.Id);
			}

			return this;
		}
		public Transform2D CreateTransform()
		{
			Transform2D transform;
			lock (pSync)
			{
				var id = Acquire();
				transform = new Transform2D(this, id);
				Transform2DData data = new()
				{
					Component = transform
				};
				pData[id] = data;
			}

			return transform;
		}

		public void GetPosition(Transform2D transform, out Vector2 position)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				position = data.Position;
			}
		}
		public void SetPosition(Transform2D transform, in Vector2 position)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				data.Dirty |= data.Position != position;
				data.Position = position;
				pData[transform.Id] = data;
			}
		}
		public void GetZIndex(Transform2D transform, out int zIndex)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				zIndex = data.ZIndex;
			}
		}
		public void SetZIndex(Transform2D transform, int zIndex)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				data.Dirty |= zIndex != data.ZIndex;
				data.ZIndex = zIndex;
				pData[transform.Id] = data;
			}
		}
		public void GetScale(Transform2D transform, out Vector2 scale)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				scale = data.Scale;
			}
		}
		public void SetScale(Transform2D transform, in Vector2 scale)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				data.Dirty |= data.Scale != scale;
				data.Scale = scale;
				pData[transform.Id] = data;
			}
		}
		public void GetRotation(Transform2D transform,out float rotation)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				rotation = data.Rotation;
			}
		}
		public void SetRotation(Transform2D transform, float rotation)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				data.Dirty |= Math.Abs(data.Rotation - rotation) > float.Epsilon;
				data.Rotation = rotation;
				pData[transform.Id] = data;
			}
		}
		public Transform2D? GetParent(Transform2D transform)
		{
			Transform2D? parent = null;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				var data = pData[transform.Id];
				if(data.ParentId != -1)
					parent = pData[data.ParentId].Component;
			}

			return parent;
		}
		public void AddChild(Transform2D transform, Transform2D child)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
				ValidateComponent(child);
#endif
				var transformData = pData[transform.Id];
				var childData = pData[child.Id];

				childData.ParentId = transform.Id;
				if(transformData.Children.Contains(child.Id))
					transformData.Children.Add(child.Id);

				transformData.Dirty = true;

				pData[transform.Id] = transformData;
				pData[child.Id] = childData;
			}
		}
		public void RemoveChild(Transform2D transform, Transform2D child)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
				ValidateComponent(child);
#endif
				var transformData = pData[transform.Id];
				var childData = pData[transform.Id];

				if (transformData.Children.Remove(child.Id))
					childData.ParentId = -1;

				pData[transform.Id] = childData;
				pData[child.Id] = childData;
			}
		}
		public void GetTransformMatrix(Transform2D transform, out Matrix4x4 matrix)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				if (pData[transform.Id].Dirty)
					UpdateTransforms(transform.Id);
				matrix = pData[transform.Id].CachedTransformMatrix;
			}
		}
		public void GetWorldTransform(Transform2D transform, out Matrix4x4 matrix)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				if(pData[transform.Id].Dirty)
					UpdateTransforms(transform.Id);
				matrix = pData[transform.Id].CachedWorldTransformMatrix;
			}
		}

		public void GetWorldPosition(Transform2D transform, out Vector2 position)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				if (pData[transform.Id].Dirty)
					UpdateTransforms(transform.Id);
				var translation = pData[transform.Id].CachedWorldTransformMatrix.Translation;
				position = new Vector2(translation.X, translation.Y);
			}
		}

		public void GetWorldRotation(Transform2D transform, out float rotation)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				if (pData[transform.Id].Dirty)
					UpdateTransforms(transform.Id);
				rotation = pData[transform.Id].WorldRotation;
			}
		}

		public IEnumerable<Transform2D> GetChildren(Transform2D transform)
		{
			IEnumerable<Transform2D> children;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				children = pData[transform.Id]
					.Children
					.Select(x => pData[x].Component ?? throw new NullReferenceException($"Child item {x} is null"));

			}

			return children;
		}

		private void UpdateTransforms(int id)
		{
			var data = pData[id];
			if (!data.Dirty)
				return;

			data.CachedTransformMatrix = Matrix4x4.CreateScale(new Vector3(data.Scale, 1))
			                             * Matrix4x4.CreateRotationZ(data.Rotation)
			                             * Matrix4x4.CreateTranslation(new Vector3(data.Position, data.ZIndex));
			data.CachedWorldTransformMatrix = data.CachedTransformMatrix;
			data.WorldRotation = data.Rotation;
			if (data.ParentId >= 0)
			{
				UpdateTransforms(data.ParentId);
				var parent = pData[data.ParentId];
				data.CachedWorldTransformMatrix = parent.CachedWorldTransformMatrix * data.CachedTransformMatrix;
				data.WorldRotation = parent.WorldRotation + data.Rotation;
			}

			data.Dirty = false;
			pData[id] = data;
		}

#if DEBUG
		private void ValidateComponent(Transform2D transform)
		{
			ValidateId(transform.Id);
			if (transform.IsDisposed)
				throw new ObjectDisposedException(nameof(Transform2D));
			if (pData[transform.Id].Component != transform)
				throw new InvalidOperationException("Invalid Component");
		}
#endif

		public Transform2D[] GetTransforms()
		{
			lock (pSync)
			{
				var transforms = new Transform2D[pData.Length - pAvailableIdx.Count];
				var nextId = 0;

				for (var i = 0; i < pData.Length; ++i)
				{
					var component = pData[i].Component;
					if(component is null)
						continue;
					transforms[nextId++] = component;
				}
				return transforms;
			}
		}
		
		public void ForEach(Action<Transform2D> action)
		{
			foreach (var transform in this)
				action(transform);
		}

		public IEnumerator<Transform2D> GetEnumerator()
		{
			lock (pSync)
			{
				if(pAvailableIdx.Count == pData.Length)
					yield break;

				foreach (var data in pData)
				{
					if(data.Component is null)
						continue;
					yield return data.Component;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
