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
		class UnresolvedTaskNode
		{
			public TaskNode Node { get; set; }
			public int NodeTarget { get; set; }

			public UnresolvedTaskNode(TaskNode node, int targetNode)
			{
				Node = node;
				NodeTarget = targetNode;
			}
		}

		private List<UnresolvedTaskNode> pTasksToResolve = new();

		public List<EPNode> Nodes { get; private set; } = new ();
		public Dictionary<int, EPNode> NodesTable { get; private set; } = new ();

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

			// Resolve references after parse
			foreach(var unresolvedTask in pTasksToResolve)
			{
				EPNode? targetNode;
				NodesTable.TryGetValue(unresolvedTask.NodeTarget, out targetNode);
				unresolvedTask.Node.Target = targetNode;
			}
		}

		private void ParseNodesRecursive(EPNode? parentNode, XmlElement element)
		{
			EPNode node = GetEPNode(element);
			node.Parent ??= parentNode;

			if (parentNode is null)
				Nodes.Add(node);
			else
				parentNode.Children.Add(node);

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

			int id = element.GetAttribute("id").GetHashCode();
		
			if (NodesTable.ContainsKey(id))
				return NodesTable[id];

			if (string.Equals(element.Name, "step"))
			{
				StepNode stepNode = StepNode.Resolve(element);
				NodesTable.Add(stepNode.Id, stepNode);
				return stepNode;
			}
			else if (string.Equals(element.Name, "task"))
			{
				TaskNode taskNode = TaskNode.Resolve(element, out int targetNodeId);
				NodesTable.Add(taskNode.Id, taskNode);

				if (targetNodeId != 0)
					pTasksToResolve.Add(new UnresolvedTaskNode(taskNode, targetNodeId));

				return taskNode;
			}

			throw new NotImplementedException($"Not implemented this element type: {element.Name}");
		}

		private static void IsValidTag(string? tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				throw new Exception("Invalid Element Name. Tag name must not be null or empty.");
			if (!(string.Equals(tagName, "step") || string.Equals(tagName, "task")))
				throw new Exception("Invalid Element Name. Tag name must be <step/> or <task/>");
		}
	}
}
