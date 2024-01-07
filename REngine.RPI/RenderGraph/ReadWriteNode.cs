using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RPI.RenderGraph
{
	public abstract class ReadWriteNode : RenderGraphNode
	{
		const string NamePropKey = "name";
		const string ValuePropKey = "value";

		private static readonly ulong NamePropHash = Hash.Digest(NamePropKey);
		private static readonly ulong ValuePropHash = Hash.Digest(ValuePropKey);

		private bool pSetup = false;
		private string pName = string.Empty;
		private string pValue = string.Empty;
		private IResourceManager? pResourceMgr;

		public IResourceManager ResourceManager
		{
			get => pResourceMgr ?? throw new NullReferenceException("Resource Manager is null.");
		}

		protected ReadWriteNode(string debugName) : base(debugName)
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if(!properties.TryGetValue(NamePropHash, out string? name))
				throw new RequiredNodePropertyException(NamePropKey, nameof(ReadWriteNode));
			if(!properties.TryGetValue(ValuePropHash, out string? value))
				throw new RequiredNodePropertyException(ValuePropKey, nameof(ReadWriteNode));
			pName = name;
			pValue = value;
		}

		protected override void OnRun(IServiceProvider provider)
		{
#if DEBUG
			if (Parent is null)
				throw new NullReferenceException("Read/Write Node must have a parent");
			if (!Parent.GetType().IsAssignableTo(typeof(RenderFeatureNode)))
				throw new RenderGraphException($"Read/Write Node parent must be {nameof(RenderFeatureNode)} inherit type");
#endif

			if(!pSetup)
			{
				if(pResourceMgr is null)
					pResourceMgr = provider.Get<IResourceManager>();
				
				RenderFeatureNode parent = (RenderFeatureNode)Parent;
				OnValue(parent, pName, pValue);
				pSetup = true;
			}
		}

		protected abstract void OnValue(RenderFeatureNode parent, string name, string value);
	}

	[NodeTag("read")]
	public sealed class ReadNode : ReadWriteNode
	{
		public ReadNode() : base(nameof(ReadNode))
		{
		}

		protected override void OnValue(RenderFeatureNode parent, string name, string value)
		{
			parent.AddReadResource(Hash.Digest(name), ResourceManager.GetResource(value));
		}
	}
	[NodeTag("write")]
	public sealed class WriteNode : ReadWriteNode
	{
		public WriteNode() : base(nameof(WriteNode))
		{
		}

		protected override void OnValue(RenderFeatureNode parent, string name, string value)
		{
			parent.AddWriteResource(Hash.Digest(name), ResourceManager.GetResource(value));
		}
	}
}
