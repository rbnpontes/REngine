using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.RPI.RenderGraph
{
	internal class RenderGraphResolver
	{
		private readonly RenderGraphRegistry pRegistry;
		
		private Dictionary<ulong, RenderGraphNode> pNodes = new();
		private RootGraphNode pRoot = new();
		private ulong idCounter = 0;

		public RenderGraphResolver(
			RenderGraphRegistry registry
		)
		{
			pRegistry = registry;
		}

		public RenderGraphEntry Load(Stream stream)
		{
			XmlDocument document = new();
			document.Load(stream);

			XmlElement? root = document.DocumentElement;
			if (root is null)
				throw new NullReferenceException("Invalid XML file. Root is null");
			if (!string.Equals(root.Name, "render-graph"))
				throw new RenderGraphException("Invalid XML file. Root Element must be <render-graph>...</render-graph>");

			for(int i =0; i < root.ChildNodes.Count; ++i)
			{
				XmlElement? element = root.ChildNodes[i] as XmlElement;
				if (element is null)
					continue;
				ParseNodesRecursive(pRoot, element);
			}

			return new RenderGraphEntry
			{
				Nodes = pNodes,
				Root = pRoot
			};
		}

		private void ParseNodesRecursive(RenderGraphNode parentNode, XmlElement element)
		{
			RenderGraphNode node = GetRenderGraphNode(element);
			parentNode.AddNode(node);

			for(int i =0; i < element.ChildNodes.Count; ++i)
			{
				XmlElement? childElement = element.ChildNodes[i] as XmlElement;
				if (childElement is null)
					continue;
				ParseNodesRecursive(node, childElement);
			}
		}

		private RenderGraphNode GetRenderGraphNode(XmlElement element)
		{
			if (string.IsNullOrEmpty(element.Name))
				throw new RenderGraphException("Invalid Element Name. Tag name must not be null or empty.");
			
			string id = element.GetAttribute("id");
			ulong idHashCode = Hash.Digest(id);

			if (pNodes.ContainsKey(idHashCode))
				throw new RenderGraphException($"Duplicated Id Entry. There´s a node with id {idHashCode}.");

			idCounter++;
			var node = pRegistry.Create(element.Name);
#if DEBUG
			node.Xml = element.OuterXml;
#endif
			node.Id = string.IsNullOrEmpty(id) ? idCounter : idHashCode;
			node.Setup(GetElementProperties(element));

			pNodes.Add(node.Id, node);
			return node;	
		}
		private Dictionary<ulong, string> GetElementProperties(XmlElement element)
		{
			Dictionary<ulong, string> properties = new();
			for (int i = 0; i < element.Attributes.Count; ++i)
			{
				var attr = element.Attributes[i];
				properties[Hash.Digest(attr.Name)] = attr.Value;
			}

			return properties;
		}
	}
}
