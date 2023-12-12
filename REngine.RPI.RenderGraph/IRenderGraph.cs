using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public struct RenderGraphEntry
	{
		public Dictionary<ulong, RenderGraphNode> Nodes;
		public RootGraphNode Root;
	}
	public interface IRenderGraph
	{
		public RenderGraphEntry? RootEntry { get; set; }
		public RenderGraphEntry Load(string assetPath);
		public RenderGraphEntry Load(Stream stream);
		/// <summary>
		/// Execute Render Graph. You must set an RootEntry first
		/// Best way to call this code is through RenderFeature
		/// </summary>
		/// <returns>self instance</returns>
		public IRenderGraph Execute();
		public RenderGraphNode? FindNode(string nodeName);
		public RenderGraphNode? FindNode(ulong nodeId);
	}
}
