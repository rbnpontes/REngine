using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.SceneManagement
{
	public class SceneObjectChildChangeEventArgs : EventArgs
	{
		public SceneObject Target { get; private set; }
		public SceneObjectChildChangeEventArgs(SceneObject target)
		{
			Target = target;
		}
	}
	public class SceneObjectParentChangeEventArgs : EventArgs 
	{ 
		public SceneObject? OldParent { get; private set; }
		public SceneObject? NewParent { get; private set; }
		public SceneObjectParentChangeEventArgs(SceneObject? oldParent, SceneObject? newParent)
		{
			OldParent = oldParent;
			NewParent = newParent;
		}
	}
	
	public class SceneObject : IDisposable
	{
		private static int sNextGlobalId = 0;

		private bool pDisposed = false;

		private bool pEnabled = false;
		private string pName = string.Empty;
		private HashSet<string> pTags = new();

		protected Scene? mScene;
		private SceneObject? pParent;
		private List<SceneObject> pChildren = new List<SceneObject>();

		private Dictionary<Type, ISceneComponent> pComponents = new();

		public int Id { get; internal set; }
		public bool Enabled
		{
			get
			{
				bool enabled = pEnabled;
				if (pParent != null && !pParent.Enabled)
					enabled = false;
				return enabled;
			}

			set => SetEnabled(value);
		}
		public string Name
		{
			get => pName;
			set => pName = value;
		}
		public Transform Transform { get; private set; } = new();
		public SceneObject? Parent 
		{
			get => pParent; 
			private set
			{
				if(pParent != value)
				{
					var args = new SceneObjectParentChangeEventArgs(pParent, value);
					pParent = value;
					OnParentChange?.Invoke(this, args);
				}
			}
		}

		public IEnumerable<SceneObject> Children { get => pChildren; }

		public IEnumerable<string> Tags { get => pTags.AsEnumerable(); }

		public Scene Scene 
		{
			get
			{
				if (mScene is null)
					throw new NullReferenceException("Scene was not set");
				return mScene;
			}
		}

		public event EventHandler? OnChangeVisibility;
		public event EventHandler<SceneObjectChildChangeEventArgs>? OnAddChild;
		public event EventHandler<SceneObjectChildChangeEventArgs>? OnRemoveChild;
		public event EventHandler<SceneObjectParentChangeEventArgs>? OnParentChange;

		public SceneObject(Scene scene)
		{
			mScene = scene;
			Id = sNextGlobalId++;
		}

		/// <summary>
		/// Create Scene Object from an Existen Transform
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="transform"></param>
		internal SceneObject(Scene scene, Transform transform)
		{
			mScene = scene;
			Transform = transform;
		}

		internal SceneObject(int id)
		{
			Id = id;
		}

		public void AddChild(SceneObject child)
		{
			AssertDispose();
			if (Scene != child.Scene)
				throw new ArgumentException("Can´t add child from another scene");

			if (child.Parent?.pChildren.Remove(child) == true)
				child.Parent?.EmitRemoveChild(child);

			child.Parent = this;
			child.Transform.Parent = Transform;

			pChildren.Add(child);
			EmitAddChild(child);
		}

		public bool RemoveChild(SceneObject child)
		{
			AssertDispose();
			if (Scene != child.Scene)
				return false;
			bool result = pChildren.Remove(child);

			child.Parent = Scene;
			child.Transform.Parent = Scene.Transform;

			Scene.pChildren.Add(child);
			EmitAddChild(child);

			EmitRemoveChild(child);
			return result;
		}

		public SceneObject CreateChild(string? name = null)
		{
			AssertDispose();
			SceneObject obj = new SceneObject(Scene);
			obj.Name = name ?? string.Empty;
			AddChild(obj);
			return obj;
		}

		public void AddTag(string tag)
		{
			AssertDispose();
			if (pTags.Contains(tag))
				return;
			pTags.Add(tag);
		}
		
		public void RemoveTag(string tag)
		{
			AssertDispose();
			pTags.Remove(tag);
		}
	
		public bool ContainsTag(string tag)
		{
			return pTags.Contains(tag);
		}

		public ISceneComponent AddComponent(ISceneComponent component)
		{
			component.Attach(this);
			if (pComponents.TryGetValue(component.GetType(), out var currComponent))
				currComponent.Detach();
			pComponents[component.GetType()] = component;
			return component;
		}
		
		public bool RemoveComponent(ISceneComponent component)
		{
			if(pComponents.TryGetValue(component.GetType(), out var currComponent))
			{
				currComponent.Detach();
				pComponents.Remove(component.GetType());
				return true;
			}
			return false;
		}
		
		public bool RemoveComponent<T>() where T : ISceneComponent
		{
			return pComponents.Remove(typeof(T));
		}
		
		public IEnumerable<ISceneComponent> GetComponents()
		{
			return pComponents.Values;
		}
		
		public IEnumerable<ISceneComponent> GetAllComponents()
		{
			IEnumerable<ISceneComponent>[] components = new IEnumerable<ISceneComponent>[pChildren.Count + 1];
			components[pChildren.Count] = GetComponents();

			for (int i = 0; i < components.Length; ++i)
				components[i] = pChildren[i].GetAllComponents();

			IEnumerable<ISceneComponent> final = Array.Empty<ISceneComponent>();
			for (int i = 0; i < components.Count(); ++i)
				final = final.Concat(components[i]);
			return final;
		}

		public IEnumerable<ISceneComponent> GetAllComponentsByTag(string tag)
		{
			return GetAllComponentsByTags(new string[] { tag });
		}
		
		public IEnumerable<ISceneComponent> GetAllComponentsByTags(IEnumerable<string> tags)
		{
			List<SceneObject> children = new List<SceneObject>();

			pChildren.ForEach(child =>
			{
				bool hasTag = false;
				foreach (string tag in tags)
				{
					if (child.ContainsTag(tag))
					{
						hasTag = true;
						break;
					}
				}

				if (hasTag)
					children.Add(child);
			});

			bool selfHasTag = false;
			foreach(string tag in tags)
			{
				if (ContainsTag(tag))
				{
					selfHasTag = true;
					break;
				}
			}

			IEnumerable<ISceneComponent>[] components = new IEnumerable<ISceneComponent>[children.Count + (selfHasTag ? 1 : 0)];
			if (selfHasTag)
				components[components.Length - 1] = GetComponents();

			for(int i =0; i < children.Count; ++i)
				components[i] = children[i].GetAllComponentsByTags(tags);

			IEnumerable<ISceneComponent> final = Array.Empty<ISceneComponent>();
			for (int i = 0; i < components.Length; ++i)
				final = final.Concat(components[i]);
			return final;
		}

		private void EmitAddChild(SceneObject obj)
		{
			OnAddChild?.Invoke(this, new SceneObjectChildChangeEventArgs(obj));
		}
		private void EmitRemoveChild(SceneObject obj)
		{
			OnRemoveChild?.Invoke(this, new SceneObjectChildChangeEventArgs(obj));
		}
		private void EmitVisibilityChange()
		{
			OnChangeVisibility?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			foreach(var pair in pComponents)
				pair.Value.Dispose();
			pComponents.Clear();
			pChildren.ForEach(x => x.Dispose());
			pChildren.Clear();

			GC.SuppressFinalize(this);
		}

		private void SetEnabled(bool enabled)
		{
			if (pEnabled == enabled)
				return;
			
			pEnabled = enabled;
			EmitVisibilityChange();

			pChildren.ForEach(x =>
			{
				x.OnParentVisibilityChange(enabled);
			});

			foreach(var pair in pComponents)
				pair.Value.OnOwnerChangeVisibility(enabled);

		}
		private void OnParentVisibilityChange(bool enabled)
		{
			EmitVisibilityChange();

			pChildren.ForEach(x => x.OnParentVisibilityChange(enabled));
			foreach (var pair in pComponents)
				pair.Value.OnOwnerChangeVisibility(enabled);
		}

		private void AssertDispose()
		{
			if (pDisposed)
				throw new ObjectDisposedException("SceneObject has been already disposed.");
		}
	}

	public sealed class Scene : SceneObject
	{
		public Scene() : base(0)
		{
			mScene = this;
		}
	}
}
