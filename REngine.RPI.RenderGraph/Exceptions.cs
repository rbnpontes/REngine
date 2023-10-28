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
		public ResourceType Expected { get; private set; }
		public ResourceType Current { get; private set; }
        public ExpectedResourceTypeException(ResourceType curr, ResourceType expected) : base($"Expected '{expected}', but gets '{curr}'")
		{
			Expected = expected;
			Current = curr;
		}
    }
	public class NotFoundVarResolver : RenderGraphException
	{
		public NotFoundVarResolver(string resolverType) : base($"Not found resolver type '{resolverType}'") { }
	}
}
