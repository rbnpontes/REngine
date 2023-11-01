using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public sealed class RenderGraphRegistry
	{
		private Dictionary<string, Type> pTypes = new Dictionary<string, Type>();

		public RenderGraphRegistry Register<T>() where T : RenderGraphNode
		{
			Type type = typeof(T);
			var attr = type.GetCustomAttribute<NodeTagAttribute>();
			if (attr is null)
				throw new NullReferenceException($"Can´t register type '{type.FullName}' because Node Tag has not been setted. Did you forget to add [NodeTag] ?");

			pTypes.Add(attr.TagName, type);
			return this;
		}

		public RenderGraphNode Create(string tagName)
		{
			if (!pTypes.TryGetValue(tagName, out Type? type))
				throw new NotRegisteredNode(tagName);

			var obj = Activator.CreateInstance(type);
			if (obj is null)
				throw new NullReferenceException($"Can´t create {type.FullName} type.");
			return (RenderGraphNode)obj;
		}
	}
}
