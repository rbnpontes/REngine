﻿using REngine.Core.Mathematics;
using REngine.Core.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public struct TransformData
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Scale;
		public Matrix4x4 CachedTransformMatrix;
		public Matrix4x4 CachedWorldTransformMatrix;
		public int ParentId;
		public List<int> Children;
		public bool Dirty;
		public Transform? Component;

		public TransformData()
		{
			Position = Vector3.Zero;
			Rotation = Quaternion.Identity;
			Scale = Vector3.One;

			CachedTransformMatrix = Matrix4x4.Identity;
			CachedWorldTransformMatrix = Matrix4x4.Identity;
			ParentId = -1;
			Children = new List<int>();
			Component = null;
			Dirty = true;
		}
	}

	public sealed class TransformSystem : BaseSystem<TransformData>, IEnumerable<Transform>
	{
		private readonly object pSync = new();

		public event EventHandler? OnMove;
		public event EventHandler? OnRotate;
		public event EventHandler? OnScale;

		public TransformSystem Destroy(Transform transform)
		{
			return transform.IsDisposed ? this : Destroy(transform.Id);
		}
		public TransformSystem Destroy(int id)
		{
			lock (pSync) {
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				// If component is null, then its already destroyed
				if (data.Component is null)
					return this;

				data.Component.Detach();

				if (data.ParentId != -1)
				{
					var parent = pData[data.ParentId];
					parent.Children.Remove(id);
					pData[data.ParentId] = parent;
				}

				var childList = data.Children;
				foreach (var child in childList)
				{
					var childItem = pData[child];
					// Move child to current parent
					childItem.ParentId = data.ParentId;
					childItem.Dirty = true;
					pData[child] = childItem;
				}
				
				pData[id] = new TransformData();

				pAvailableIdx.Enqueue(id);
			}

			return this;
		}
		public Transform CreateTransform()
		{
			Transform transform;
			lock (pSync)
			{
				var id = Acquire();
				transform = new Transform(this, id);
				TransformData data = new ()
				{
					Component = transform
				};
				pData[id] = data;
			}

			return transform;
		}

		public void GetPosition(Transform transform, out Vector3 position)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			GetPosition(transform.Id, out position);
		}
		public void GetPosition(int id, out Vector3 position)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				position = pData[id].Position;
			}
		}
		public void SetPosition(Transform transform, in Vector3 position)
		{
#if DEBUG
			lock(pSync)
				ValidateComponent(transform);
#endif
			SetPosition(transform.Id, position);
		}
		public void SetPosition(int id, in Vector3 position)
		{
			Transform? transform = null;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.Position = position;
				transform = data.Component;
				pData[id] = data;

				MarkDirty(id);
			}

			OnMove?.Invoke(transform, EventArgs.Empty);
		}
		public void GetRotation(Transform transform, out Quaternion rotation)
		{
#if DEBUG
			lock(pSync)
				ValidateComponent(transform);
#endif
			GetRotation(transform.Id, out rotation);
		}
		public void GetRotation(int id, out Quaternion rotation)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				rotation = pData[id].Rotation;
			}
		}
		public void SetRotation(Transform transform, in Quaternion rotation)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			SetRotation(transform.Id, rotation);
		}
		public void SetRotation(int id, in Quaternion rotation)
		{
			Transform? transform = null;

			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.Rotation = rotation;
				transform = data.Component;
				pData[id] = data;

				MarkDirty(id);
			}

			OnRotate?.Invoke(transform, EventArgs.Empty);
		}
		public void GetScale(Transform transform, out Vector3 scale)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(transform);
#endif
				scale = pData[transform.Id].Scale;
			}
		}
		public void SetScale(Transform transform, in Vector3 scale)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			SetScale(transform.Id, scale);
		}
		public void SetScale(int id, in Vector3 scale)
		{
			Transform? transform;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				data.Scale = scale;
				transform = data.Component;
				pData[id] = data;

				MarkDirty(id);
			}
			
			OnScale?.Invoke(transform, EventArgs.Empty);
		}

		public void GetTransformMatrix(Transform transform, out Matrix4x4 matrix)
		{
#if DEBUG
			lock(pSync)
				ValidateComponent(transform);
#endif
			GetTransformMatrix(transform.Id, out matrix);
		}
		public void GetTransformMatrix(int id, out Matrix4x4 matrix)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				if (pData[id].Dirty)
					UpdateTransforms(id);
				matrix = pData[id].CachedTransformMatrix;
			}
		}
		public void GetWorldTransformMatrix(Transform transform, out Matrix4x4 matrix)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			GetWorldTransformMatrix(transform.Id, out matrix);
		}
		public void GetWorldTransformMatrix(int id, out Matrix4x4 matrix)
		{
            lock (pSync)
            {
#if DEBUG
				ValidateComponent(id);
#endif
				if (pData[id].Dirty)
					UpdateTransforms(id);
				matrix = pData[id].CachedWorldTransformMatrix;
			}
        } 

		public void GetEulerAngles(Transform transform, out Vector3 rotation)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			GetEulerAngles(transform.Id, out rotation);
		}
		public void GetEulerAngles(int id,out Vector3 rotation)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				rotation = pData[id].Rotation.ToEulerAngles();
			}
		}
		public void SetEulerAngles(Transform transform, in Vector3 rotation)
		{
#if DEBUG
			lock(pSync)
				ValidateComponent(transform);
#endif
			SetEulerAngles(transform.Id, rotation);
		}
		public void SetEulerAngles(int id, in Vector3 rotation)
		{
			Transform? transform;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var rot = rotation.FromEulerAngles();
				var data = pData[id];

				data.Rotation = rot;
				pData[id] = data;
				transform = pData[id].Component;

				MarkDirty(id);
			}

			OnRotate?.Invoke(transform, EventArgs.Empty);
		}

		public Transform? GetParent(Transform transform)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			return GetParent(transform.Id);
		}
		public Transform? GetParent(int id)
		{
			Transform? parent = null;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				var data = pData[id];
				if (data.ParentId != -1)
					parent = pData[data.ParentId].Component;
			}

			return parent;
		}

		public void AddChild(Transform transform, Transform child)
		{
#if DEBUG
			lock (pSync)
			{
				ValidateComponent(transform);
				ValidateComponent(child);
			}
#endif
			AddChild(transform.Id, child.Id);
		}
		public void AddChild(int id, int childId)
		{
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
				ValidateComponent(childId);
#endif
				var target = pData[id];
				var child = pData[childId];

				if (target.Children.Contains(childId))
					return;
				
				child.ParentId = id;
				target.Children.Add(childId);

				child.Dirty = true;

				pData[id] = target;
				pData[childId] = child;

				MarkDirty(childId);
			}
		}

		public void RemoveChild(Transform transform, Transform child) 
		{
#if DEBUG
			lock (pSync)
			{
				ValidateComponent(transform);
				ValidateComponent(child);
			}
#endif

			RemoveChild(transform.Id, child.Id);
		}
		public void RemoveChild(int id, int childId)
		{
			lock(pSync)
			{
#if DEBUG
				ValidateComponent(id);
				ValidateComponent(childId);
#endif
				var transform = pData[id];
				var child = pData[childId];

				if(transform.Children.Remove(childId))
					child.ParentId = -1;

				pData[id] = transform;
				pData[childId] = child;

				MarkDirty(childId);
			}
		}

		public IEnumerable<Transform> GetChildren(Transform transform)
		{
#if DEBUG
			lock (pSync)
				ValidateComponent(transform);
#endif
			return GetChildren(transform.Id);
		}
		
		public IEnumerable<Transform> GetChildren(int id)
		{
			IEnumerable<Transform> children;
			lock (pSync)
			{
#if DEBUG
				ValidateComponent(id);
#endif
				children = pData[id]
						.Children
						.Select(x => pData[x].Component ?? throw new NullReferenceException($"Child item {x} is null"));
			}

			return children;
		}

		private void MarkDirty(int id)
		{
			var data = pData[id];
			if (data.Dirty)
				return;

			data.Dirty = true;
			pData[id] = data;
			data.Children.ForEach(MarkDirty);
		}

		private void UpdateTransforms(int id)
		{
			var data = pData[id];
			data.CachedTransformMatrix = Matrix4x4.CreateScale(data.Scale)
				* Matrix4x4.CreateFromQuaternion(data.Rotation)
				* Matrix4x4.CreateTranslation(data.Position);

			data.CachedWorldTransformMatrix = data.CachedTransformMatrix;
			if(data.ParentId >= 0)
			{
				UpdateTransforms(data.ParentId);
				var parent = pData[data.ParentId];
				data.CachedWorldTransformMatrix = data.CachedTransformMatrix * parent.CachedWorldTransformMatrix;
			}

			data.Dirty = false;
			pData[id] = data;
		}

		protected override int GetExpansionSize()
		{
			return 1;
		}
#if DEBUG
		private void ValidateComponent(Transform transform)
		{
			ValidateComponent(transform.Id);
			if (transform.IsDisposed)
				throw new ObjectDisposedException(nameof(Transform));
			if (pData[transform.Id].Component != transform)
				throw new Exception("Invalid Component");
		}

		private void ValidateComponent(int id)
		{
			ValidateId(id);
		}
#endif
		public Transform[] GetTransforms()
		{
			var transforms = new Transform[pData.Length - pAvailableIdx.Count];
			int nextId = 0;

			for(int i =0;i < pData.Length; ++i)
			{
				var component = pData[i].Component;
				if (component is null)
					continue;
				transforms[nextId] = component;
				++nextId;
			}
			return transforms;
		}

		public Transform? GetTransform(int id)
		{
			if (id < 0 || id >= pData.Length)
				return null;
			return pData[id].Component;
		}

		public void ForEach(Action<Transform> action)
		{
			foreach(var transform in this)
				action(transform);
		}

		public IEnumerator<Transform> GetEnumerator()
		{
			lock (pSync)
			{
				if (pAvailableIdx.Count == pData.Length)
					yield break;

				foreach(var data in pData)
				{
					if (data.Component is null)
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
