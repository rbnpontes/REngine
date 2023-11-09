using REngine.Core.Mathematics;
using REngine.Core.Threading.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace REngine.Core.Threading
{
	internal class EPResolver
	{
		// TODO: Refactor this resolver
		// This class must be more resilient 
		// And next feature on this class must include
		// dynamic changes

		class UnresolvedTaskNode
		{
			public TaskNode Node { get; set; }
			public ulong NodeTarget { get; set; }

			public UnresolvedTaskNode(TaskNode node, ulong targetNode)
			{
				Node = node;
				NodeTarget = targetNode;
			}
		}

		private readonly ExecutionPipelineNodeRegistry pNodeRegistry;
		private readonly ExecutionPipelineImpl pPipeline;

		private List<(XmlElement, EPNode)> pCreatedNodes = new();

		public List<EPNode> RootNodes { get; private set; } = new();
		public Dictionary<ulong, EPNode> NodesTable { get; private set; } = new ();

		public EPResolver(ExecutionPipelineNodeRegistry registry, ExecutionPipelineImpl pipeline)
		{
			pNodeRegistry = registry;
			pPipeline = pipeline;
		}

		public void Load(Stream stream)
		{
			XmlDocument document = new();
			document.Load(stream);

			XmlElement? root = document.DocumentElement;
			if (root is null)
				throw new NullReferenceException("Invalid XML file. Root is null");
			if (!string.Equals(root.Name, "pipeline"))
				throw new Exception("Invalid XML file. Root Element must be <pipeline>...</pipeline>");

			for(int i =0; i < root.ChildNodes.Count; ++i)
			{
				XmlElement? element = root.ChildNodes[i] as XmlElement;
				if (element is null)
					continue;
				ParseNodesRecursive(null, element);
			}

			// Resolve and Define Nodes
			foreach(var pair in pCreatedNodes)
			{
				(XmlElement element, EPNode node) = pair;
				node.Define(element, NodesTable);
			}
		}

		private void ParseNodesRecursive(EPNode? parentNode, XmlElement element)
		{
			EPNode node = GetEPNode(element);
			node.Parent ??= parentNode;

			if (parentNode is null)
				RootNodes.Add(node);
			else
				parentNode.Children.Add(node);

			pCreatedNodes.Add((element, node));

			for(int i = 0; i < element.ChildNodes.Count; ++i)
			{
				XmlElement? childElement = element.ChildNodes[i] as XmlElement;
				if (childElement is null)
					continue;
				ParseNodesRecursive(node, childElement);
			}
		}

		private EPNode GetEPNode(XmlElement element)
		{
			IsValidTag(element.Name);

			string idValue = element.GetAttribute("id");
			ulong id = Hash.Digest(idValue);

			if (NodesTable.ContainsKey(id))
				throw new Exception($"Node with '{id}' is already registered.");

			EPNode node = pNodeRegistry.Create(element.Name, pPipeline);
			node.Id = id;
#if DEBUG
			node.DebugName = idValue;
			node.Xml = element.OuterXml;
#endif
			if(id != 0)
				NodesTable[id] = node;
			return node;
		}

		private static void IsValidTag(string? tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				throw new Exception("Invalid Element Name. Tag name must not be null or empty.");
		}
	}
}
