﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public abstract class Component : IDisposable
	{
		private bool pEnabled = false;
		private bool pDisposed = false;
		private Entity? pOwner;

		public bool Enabled 
		{
			get 
			{
				bool enabled = pEnabled;
				if(Owner != null && !Owner.Enabled)
					enabled = false;
				return enabled;
			}
			set
			{
				bool currState = Enabled;
				pEnabled = value;
				bool newState = Enabled;
				if (currState != newState)
					OnChangeVisibility(newState);
			}
		}

		public Entity? Owner 
		{
			get => pOwner;
			internal set => AttachEntity(value);
		}

		public bool IsDisposed { get => pDisposed; }

		public Component()
		{
		}

		private void AttachEntity(Entity target)
		{
			if (pOwner != null && pOwner != target)
				DetachEntity();

			OnAttach(target);
			pOwner = target;
		}

		private void DetachEntity()
		{
			var oldOwner = pOwner;
			pOwner = null;
			OnDetach(oldOwner);
		}

		/// <summary>
		/// Remove self component from Entity
		/// </summary>
		public void Detach()
		{
			Owner?.RemoveComponent(this);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			Detach();
			OnDispose();
			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		protected abstract void OnDispose();

		protected virtual void OnDetach(Entity? target) { }
		protected virtual void OnAttach(Entity? target) { }
		protected virtual void OnChangeVisibility(bool value) { }

		protected void ValidateDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(Component));
		}
	}
}