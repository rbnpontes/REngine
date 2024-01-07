using REngine.Core.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public sealed class Entity : IDisposable
	{
		private readonly EntityManager pEntityManager;
		private bool pDisposed;

		public int Id { get; private set; }
		public bool Enabled
		{
			get
			{
				AssertDispose();
				return pEntityManager.IsEnabled(this);
			}
			set
			{
				AssertDispose();
				pEntityManager.SetEnabled(this, value);
			}
		}
		public string Name 
		{ 
			get
			{
				AssertDispose();
				pEntityManager.GetEntityData(Id, out var data);
				return data.Name;
			}
			set
			{
				AssertDispose();
				pEntityManager.GetEntityData(Id, out var data);
				data.Name = value;
				pEntityManager.SetEntityData(Id, data);
			}
		}
		public bool IsDisposed => pDisposed;

		public IEnumerable<string> Tags 
		{ 
			get
			{
				AssertDispose();
				pEntityManager.GetEntityData(Id, out var data);
				return data.Tags.AsEnumerable();
			}
		}

		internal Entity(EntityManager entityManager, int id)
		{
			pEntityManager = entityManager;
			Id = id;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;
			pEntityManager.Destroy(this);
			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		public void AddTag(string tag)
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			if (data.Tags.Contains(tag))
				return;
			data.Tags.Add(tag);
			pEntityManager.SetEntityData(Id, data);
		}
		
		public Entity RemoveTag(string tag)
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			data.Tags.Remove(tag);
			pEntityManager.SetEntityData(Id, data);
			return this;
		}
	
		public bool ContainsTag(string tag)
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			return data.Tags.Contains(tag);
		}

		public Entity AddComponent(Component component)
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			Type componentType = component.GetType();
#if DEBUG
			if (data.Components.ContainsKey(componentType))
				throw new EntityException($"This component type '{component.GetType().Name}' has been already added to Entity.");
#endif
			data.Components[componentType] = component;
			component.Owner = this;

			pEntityManager.SetEntityData(Id, data);

			component.OnSetup();
			return this;
		}
		
		public bool RemoveComponent(Component component)
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			if(data.Components.TryGetValue(component.GetType(), out var currComponent))
			{
				if (currComponent != component)
					return false;
				data.Components.Remove(component.GetType());
				component.Owner = null;
				pEntityManager.SetEntityData(Id, data);
				return true;
			}
			return false;
		}
		
		public bool RemoveComponent<T>() where T : Component
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			bool result = data.Components.Remove(typeof(T));
			pEntityManager.SetEntityData(Id, data);
			return result;
		}
		
		public T? GetComponent<T>() where T : Component
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			data.Components.TryGetValue(typeof(T), out var component);
			return component as T;
		}

		public T CreateComponent<T>() where T : Component
		{
			AssertDispose();
			return pEntityManager.CreateComponent<T>(this);
		}

		public object CreateComponent(Type componentType)
		{
			AssertDispose();
			return pEntityManager.CreateComponent(this, componentType);
		}

		public IEnumerable<Component> GetComponents()
		{
			AssertDispose();
			pEntityManager.GetEntityData(Id, out var data);
			return data.Components.Values;
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException($"{nameof(Entity)} has been already disposed.");
		}
	}
}
