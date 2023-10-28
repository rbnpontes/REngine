using REngine.Core.DependencyInjection;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public abstract class RenderGraphNode : IDisposable
	{
		private List<RenderGraphNode> pChildren = new List<RenderGraphNode>();
		private IServiceProvider? pServiceProvider;
		private bool pHasSetup;
		private bool pDirty = true;
#if DEBUG
		public string DebugName { get; private set; }
#endif
		public bool IsDisposed { get; private set; }
		public IReadOnlyList<RenderGraphNode> Children { get => pChildren.AsReadOnly(); }
		public RenderGraphNode? Parent { get; private set; }

		public virtual bool HasSetup { get => pHasSetup; }
		public virtual bool IsDirty { get => pDirty; }

		public IServiceProvider ServiceProvider
		{
			get
			{
				if (pServiceProvider is null)
					throw new NullReferenceException("Service Provider is null, it seems Run is not called.");
				return pServiceProvider;
			}
		}

		public RenderGraphNode(string debugName)
		{
#if DEBUG
			DebugName = debugName;
#endif
		}

		public RenderGraphNode AddNode(RenderGraphNode node)
		{
			if (OnAddChild(node))
			{
				pChildren.Add(node);
				node.Parent = this;
			}
			return this;
		}
		public RenderGraphNode RemoveNode(RenderGraphNode node)
		{
			if (OnRemoveChild(node))
			{
				pChildren.Remove(node);
				node.Parent = null;
			}
			return this;
		}

		public void Run(IServiceProvider provider)
		{
			pServiceProvider = provider;
			OnRun(provider);

			var children = OnGetChildren();
			foreach ( var child in children )
				child.Run(provider);
		}

		public void Setup(IDictionary<int, string> properties) 
		{
			if (pHasSetup)
				throw new RenderGraphException("Setup has been called");
			if (IsDisposed)
				return;

			OnSetup(properties);
			pHasSetup = true;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;

			OnDispose();
			var children = OnGetChildren();
			foreach (var child in children)
				child.Dispose();

			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public virtual void MarkAsDirty()
		{
			pDirty = true;
		}

		protected void UnmarkDirty()
		{
			pDirty = false;
		}

		protected abstract void OnRun(IServiceProvider provider);
		protected virtual void OnSetup(IDictionary<int, string> properties) { }
		protected virtual bool OnAddChild(RenderGraphNode node) { return true; }
		protected virtual bool OnRemoveChild(RenderGraphNode node) { return true; }
		protected virtual IEnumerable<RenderGraphNode> OnGetChildren() { return Children; }
		protected virtual void OnDispose() { }
	}

	public abstract class ExecutableGraphNode : RenderGraphNode 
	{
		private IGraphicsDriver? pGraphicsDriver;

		public ExecutableGraphNode(string debugName) : base(debugName)
		{
		}

		public void Compile(IDevice device, ICommandBuffer command)
		{
			if (!IsDirty || IsDisposed)
				return;

			if (!HasSetup)
				throw new RenderGraphException("Setup must called first before Compile");

			OnCompile(device, command);
			UnmarkDirty();
		}

		public void Execute(ICommandBuffer command)
		{
			if (IsDirty || IsDisposed)
				return;

			if (!HasSetup)
				throw new RenderGraphException("Setup must called first before Execute");

			OnExecute(command);
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if(pGraphicsDriver is null)
				pGraphicsDriver = provider.Get<IGraphicsDriver>();

			Compile(pGraphicsDriver.Device, pGraphicsDriver.ImmediateCommand);
			Execute(pGraphicsDriver.ImmediateCommand);
		}

		protected virtual void OnCompile(IDevice device, ICommandBuffer command) { }
		protected virtual void OnExecute(ICommandBuffer command) { }
	}

	public abstract class ResourceGraphNode : RenderGraphNode
	{
		private IGraphicsDriver? pDriver;
		private IResourceManager? pResourceManager;

		public ResourceGraphNode(string debugName) : base(debugName)
		{
		}

		public void Build(IResourceManager resourceManager, IDevice device, ICommandBuffer command)
		{
			if (!IsDirty)
				return;

			var children = OnBuild(resourceManager, device, command);
			foreach (var child in children)
			{
				if(child is ResourceGraphNode node)
					node.Build(resourceManager, device, command);
			}

			UnmarkDirty();
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if (pDriver is null)
				pDriver = provider.Get<IGraphicsDriver>();
			if (pResourceManager is null)
				pResourceManager = provider.Get<IResourceManager>();

			Build(pResourceManager, pDriver.Device, pDriver.ImmediateCommand);
		}

		protected override bool OnAddChild(RenderGraphNode node)
		{
			return IsCompatibleChildNode(node);
		}
		protected override bool OnRemoveChild(RenderGraphNode node)
		{
			return IsCompatibleChildNode(node);
		}
		
		protected bool IsCompatibleChildNode(RenderGraphNode node)
		{
			return node.GetType().IsAssignableTo(typeof(ResourceGraphNode));
		}

		protected virtual IEnumerable<RenderGraphNode> OnBuild(IResourceManager resourceManager, IDevice device, ICommandBuffer command) { return Children; }
	}
}
