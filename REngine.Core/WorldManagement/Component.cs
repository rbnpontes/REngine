using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Serialization;

namespace REngine.Core.WorldManagement
{
	public abstract class Component : IDisposable
	{
		private bool pEnabled = true;
		private Entity? pOwner;

		[SerializationIgnore]
		public bool Enabled 
		{
			get 
			{
				var enabled = pEnabled;
				if(Owner is { Enabled: false })
					enabled = false;
				return enabled;
			}
			set
			{
				var currState = Enabled;
				pEnabled = value;
				var newState = Enabled;
				if (currState != newState)
					OnChangeVisibility(newState);
			}
		}

		[SerializationIgnore]
		public Entity? Owner 
		{
			get => pOwner;
			internal set => AttachEntity(value);
		}

		[SerializationIgnore]
		public bool IsDisposed { get; private set; }

		public event EventHandler? OnDestroy;

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
			if (IsDisposed)
				return;
			Detach();
			
			OnDestroy?.Invoke(this, EventArgs.Empty);
			OnDispose();
			
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		protected abstract void OnDispose();

		protected virtual void OnDetach(Entity? target) { }
		protected virtual void OnAttach(Entity? target) { }
		protected virtual void OnChangeVisibility(bool value) { }

		public virtual void OnOwnerChangeVisibility(bool value)
		{
			OnChangeVisibility(Enabled);
		}

		public virtual void OnSetup() { }

		protected void ValidateDispose()
		{
			ObjectDisposedException.ThrowIf(IsDisposed, this);
		}
	}
}
