using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph.Nodes
{
	[NodeTag("input")]
	public sealed class InputNode : RenderGraphNode
	{
		const string InputIdPropertyKey = "id";
		const string InputTypePropertyKey = "type";

		private string pId = string.Empty;

		private IResource? pResource;
		private GPUObjectType pExpectedResourceType = GPUObjectType.Unknown;

		public InputNode() : base(nameof(InputNode))
		{
		}

		protected override void OnSetup(IDictionary<int, string> properties)
		{
			if (!properties.TryGetValue(InputIdPropertyKey.GetHashCode(), out string? id))
				throw new RequiredNodePropertyException(InputIdPropertyKey, nameof(InputNode));
			if (!properties.TryGetValue(InputTypePropertyKey.GetHashCode(), out string? type))
				throw new RequiredNodePropertyException(InputTypePropertyKey, nameof(InputNode));

			string[] typeParts = type.Split('|');
			foreach(var typePart in typeParts)
			{
				GPUObjectType objType = GPUObjectType.Unknown;
				Enum.TryParse(typePart, true, out objType);
				pExpectedResourceType |= objType;
			}

			pId = id;
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if (pResource != null)
				return;
			pResource = ServiceProvider.Get<IResourceManager>().GetResource(pId);
			ValidateResource(pResource);
			pResource.ValueChanged += HandleResourceChange;
		}

		private void HandleResourceChange(object? sender, EventArgs e)
		{
			ValidateResource(pResource);
		}

		private void ValidateResource(IResource resource)
		{
			if (resource.Value is null)
				return;
			if ((resource.Value.ObjectType & pExpectedResourceType) != 0)
				throw new ExpectedResourceTypeException(resource.Value.ObjectType, pExpectedResourceType);
		}

		protected override IEnumerable<RenderGraphNode> OnGetChildren()
		{
			return Array.Empty<RenderGraphNode>();
		}

		protected override void OnDispose()
		{
			if(pResource != null)
				pResource.ValueChanged -= HandleResourceChange;
		}
	}
}
