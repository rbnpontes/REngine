using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public class RenderGraphException : Exception
	{
		public RenderGraphException(string message) : base(message) { }
	}

	public class RequiredNodePropertyException : RenderGraphException
	{
		public RequiredNodePropertyException(string propertyName, string nodeName) : base($"Expected property '{propertyName}' on '{nodeName}'")
		{
		}
	}
    public class ExpectedResourceTypeException : RenderGraphException
    {
		public GPUObjectType Expected { get; private set; }
		public GPUObjectType Current { get; private set; }
        public ExpectedResourceTypeException(GPUObjectType curr, GPUObjectType expected) : base($"Expected '{expected}', but gets '{curr}'")
		{
			Expected = expected;
			Current = curr;
		}
    }
	public class NotFoundVarResolver : RenderGraphException
	{
		public NotFoundVarResolver(string resolverType) : base($"Not found resolver type '{resolverType}'") { }
	}
	public class NotRegisteredNode : RenderGraphException
	{
		public NotRegisteredNode(string tagName) : base($"There´s no node registered for tag '{tagName}'") { }
	}
}
