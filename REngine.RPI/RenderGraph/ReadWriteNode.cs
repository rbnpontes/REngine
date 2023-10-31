using REngine.Core.DependencyInjection;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public abstract class ReadWriteNode : RenderGraphNode
	{
		const string NamePropKey = "name";
		const string ValuePropKey = "value";

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

		protected override void OnSetup(IDictionary<int, string> properties)
		{
			if(!properties.TryGetValue(NamePropKey.GetHashCode(), out string? name))
				throw new RequiredNodePropertyException(NamePropKey, nameof(ReadWriteNode));
			if(!properties.TryGetValue(ValuePropKey.GetHashCode(), out string? value))
				throw new RequiredNodePropertyException(ValuePropKey, nameof(ReadWriteNode));
			pName = name;
			pValue = value;
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if (Parent is null)
				throw new NullReferenceException("Read/Write Node must have a parent");
			if (!Parent.GetType().IsAssignableTo(typeof(RenderFeatureNode)))
				throw new RenderGraphException("Read/Write Node parent must be RenderFeatureNode inherit type");


			if(!pSetup)
			{
				if(pResourceMgr is null)
					pResourceMgr = provider.Get<IResourceManager>();
				
				RenderFeatureNode parent = (RenderFeatureNode)Parent;
				OnValue(parent, pName, pValue);
				pSetup = false;
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
			parent.AddReadResource(name.GetHashCode(), ResourceManager.GetResource(value));
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
			parent.AddWriteResource(name.GetHashCode(), ResourceManager.GetResource(value));
		}
	}
}
