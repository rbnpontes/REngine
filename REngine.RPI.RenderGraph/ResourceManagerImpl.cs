using REngine.Core.Mathematics;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	internal class ResourceManagerImpl : IResourceManager
	{
		private Dictionary<ulong, ResourceImpl> pResources = new();

		public IResourceManager AddResource(in ResourceInsertInfo resource)
		{
			ResourceImpl res = new();
			res.Id = resource.Id;
			res.Value = resource.Resource;
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
			var res = GetOrCreateResource(Hash.Digest(name));
#if DEBUG
			res.DebugName = name;
#endif
			return res;
		}

		public IResource GetResource(ulong resourceId)
		{
			return GetOrCreateResource(resourceId);
		}

		public IResourceManager UpdateResource(string resourceName, IGPUObject resource)
		{
			ResourceImpl res = GetOrCreateResource(Hash.Digest(resourceName));
#if DEBUG
			res.DebugName = resourceName;
#endif
			res.Mutate(resource);
			return this;
		}

		public IResourceManager UpdateResource(ulong resourceId, IGPUObject resource)
		{
			ResourceImpl res = GetOrCreateResource(resourceId);
			res.Mutate(resource);
			return this;
		}

		private ResourceImpl GetOrCreateResource(ulong resourceId)
		{
			if(!pResources.TryGetValue(resourceId, out ResourceImpl? res))
			{
				res = new ResourceImpl();
				res.Id = resourceId;
				pResources[resourceId] = res;
			}
			return res;
		}
	}
}
