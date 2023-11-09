using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	internal class RenderGraphImpl : IRenderGraph
	{
		private readonly RenderGraphRegistry pRegistry;
		private readonly IServiceProvider pProvider;

		public RenderGraphEntry? RootEntry { get; set; }

		public RenderGraphImpl(RenderGraphRegistry registry, IServiceProvider provider)
		{
			pRegistry = registry;
			pProvider = provider;
		}

		public IRenderGraph Execute()
		{
			if (RootEntry is null)
				throw new NullReferenceException("Can´t execute Render Graph, RootEntry is null.");
			RootEntry.Value.Root.Run(pProvider);
			return this;
		}

		public RenderGraphEntry Load(Stream stream)
		{
			RenderGraphResolver resolver = new RenderGraphResolver(pRegistry);
			return resolver.Load(stream);
		}

		public RenderGraphEntry LoadFromFile(string filePath)
		{
			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return Load(stream);
		}

		public RenderGraphNode? FindNode(string nodeName)
		{
			return FindNode(Hash.Digest(nodeName));
		}

		public RenderGraphNode? FindNode(ulong nodeId)
		{
			if (RootEntry is null)
				throw new NullReferenceException("Is not possible to found Render Graph Node, RootEntry is null.");
			RootEntry.Value.Nodes.TryGetValue(nodeId, out var node);
			return node;
		}
	}
}
