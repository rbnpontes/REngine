using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class NodeTagAttribute : Attribute
	{
		public string TagName { get; private set; }
		public NodeTagAttribute(string tagName)
		{
			TagName = tagName;
		}
	}
}
