using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	internal class ResourceManagerImpl : IResourceManager
	{
		private Dictionary<int, ResourceImpl> pResources = new();

		public IResourceManager AddResource(in ResourceInsertInfo resource)
		{
			ResourceImpl res = new();
			res.Id = resource.Id;
			res.Value = resource.Resource;
			res.Type = resource.ResourceType;
			pResources.Add(resource.Id, res);
			return this;
		}

		public IResourceManager AddResources(IEnumerable<ResourceInsertInfo> resources)
		{
			foreach(var resource in resources)
				AddResource(resource);
			return this;
		}

		public IResource GetResource(string name)
		{
			var res = GetOrCreateResource(name.GetHashCode());
#if DEBUG
			res.DebugName = name;
#endif
			return res;
		}

		public IResource GetResource(int resourceId)
		{
			return GetOrCreateResource(resourceId);
		}

		private ResourceImpl GetOrCreateResource(int resourceId)
		{
			if(!pResources.TryGetValue(resourceId, out ResourceImpl? res))
			{
				res = new ResourceImpl();
				res.Id = resourceId;
				res.Type = ResourceType.Unknow;
				pResources[resourceId] = res;
			}
			return res;
		}
	}
}
