using REngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.RPI.RenderGraph
{
	internal class RenderGraphImpl(
		RenderGraphRegistry registry, 
		IServiceProvider provider,
		IAssetManager assetManager) : IRenderGraph
	{
		public RenderGraphEntry? RootEntry { get; set; }

		public IRenderGraph Execute()
		{
			if (RootEntry is null)
				throw new NullReferenceException("Can´t execute Render Graph, RootEntry is null.");
			RootEntry.Value.Root.Run(provider);
			return this;
		}

		public RenderGraphEntry Load(Stream stream)
		{
			RenderGraphResolver resolver = new RenderGraphResolver(registry);
			return resolver.Load(stream);
		}

		public RenderGraphEntry Load(string assetPath)
		{
			return Load(assetManager.GetStream(assetPath));
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
