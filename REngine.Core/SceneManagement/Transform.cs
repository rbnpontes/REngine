using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public sealed class Transform
	{
		private readonly object pSync = false;

		private Vector3 pPosition = Vector3.Zero;
		private Quaternion pRotation = Quaternion.Identity;
		private Vector3 pScale = Vector3.One;
		private Matrix4x4 pTransformMatrix = new();
		private Matrix4x4 pWorldTransformMatrix = new();
		private Transform? pParent = null;
		private bool pDirty = true;

		public Transform? Parent
		{
			get
			{
				Transform? result;
				lock (pSync)
					result = pParent;
				return result;
			}
			set
			{
				lock (pSync)
				{
					if (pParent == value)
						return;
					UnregisterEvents();
					pParent = value;
					RegisterEvents();
					pDirty = true;
				}
			}
		}

		public Vector3 Position
		{
			get
			{
				Vector3 value;
				lock(pSync)
					value = pPosition;
				return value;
			}
			set
			{
				bool isDirty = false;
				lock (pSync)
				{
					if(pPosition != value)
						isDirty = pDirty = true;
					pPosition = value;
				}

				if (isDirty)
					OnMove?.Invoke(this, EventArgs.Empty);
			}
		}

		public Quaternion Rotation
		{
			get
			{
				Quaternion value;
				lock(pSync)
					value = pRotation;
				return value;
			}
			set
			{
				bool isDirty = false;
				lock (pSync)
				{
					if (pRotation != value)
						isDirty = pDirty = true;
					pRotation = value;
				}

				if(isDirty)
					OnRotate?.Invoke(this, EventArgs.Empty);
			}
		}

		public Vector3 Scale
		{
			get
			{
				Vector3 value;
				lock(pSync)
					value = pScale;
				return value;
			}
			set
			{
				bool isDirty = false;
				lock (pSync)
				{
					if(pScale != value)
						isDirty = pDirty = true;
					pScale = value;
				}

				if(isDirty)
					OnScale?.Invoke(this, EventArgs.Empty);
			}
		}

		public Matrix4x4 TransformMatrix
		{
			get
			{
				Matrix4x4 result;
				lock (pSync)
				{
					if (!pDirty)
						UpdateTransforms();
					result = pTransformMatrix;
				}
				return result;
			}
		}

		public Matrix4x4 WorldTransformMatrix
		{
			get
			{
				Matrix4x4 result;
				lock (pSync)
				{
					if(!pDirty)
						UpdateTransforms();
					result = pWorldTransformMatrix;
				}

				return result;
			}
		}

		public Vector3 Forward
		{
			get => Vector3.Transform(new Vector3(0, 0, +1), pRotation);
		}
		public Vector3 Backward
		{
			get => Vector3.Transform(new Vector3(0, 0, -1), pRotation);
		}
		public Vector3 Up
		{
			get => Vector3.Transform(new Vector3(0, +1, 0), pRotation);
		}
		public Vector3 Down
		{
			get => Vector3.Transform(new Vector3(0, -1, 0), pRotation);
		}
		public Vector3 Left
		{
			get => Vector3.Transform(new Vector3(-1, 0, 0), pRotation);
		}
		public Vector3 Right
		{
			get => Vector3.Transform(new Vector3(+1, 0, 0), pRotation);
		}

		public event EventHandler? OnMove;
		public event EventHandler? OnRotate;
		public event EventHandler? OnScale;

		private void UpdateTransforms()
		{
			pTransformMatrix = Matrix4x4.CreateScale(pScale)
							* Matrix4x4.CreateFromQuaternion(pRotation)
							* Matrix4x4.CreateTranslation(pPosition);

			if(Parent != null)
				pWorldTransformMatrix = Parent.WorldTransformMatrix * pTransformMatrix;
		}

		private void UnregisterEvents()
		{
			if (pParent is null)
				return;

			pParent.OnMove -= OnMove;
			pParent.OnRotate -= OnRotate;
			pParent.OnScale -= OnScale;
		}
		private void RegisterEvents()
		{
			if (pParent is null)
				return;

			pParent.OnMove += HandleMove;
			pParent.OnRotate += HandleRotate;
			pParent.OnScale += HandleScale;
		}

		private void HandleMove(object? sender, EventArgs e)
		{
			lock (pSync)
				pDirty = true;
			OnMove?.Invoke(this, EventArgs.Empty);
		}

		private void HandleRotate(object? sender, EventArgs e)
		{
			lock (pSync) 
				pDirty = true;
			OnRotate?.Invoke(this, EventArgs.Empty);
		}

		private void HandleScale(object? sender, EventArgs e)
		{
			lock (pSync)
				pDirty = true;
			OnScale?.Invoke(this, EventArgs.Empty);
		}
	}
}
