using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public struct ResourceInsertInfo
	{
		public ulong Id;
		public IGPUObject Resource;
	}
	public interface IResourceManager
	{
		public IResource GetResource(string resourceName);
		public IResource GetResource(ulong resourceId);
		public IResourceManager AddResource(in ResourceInsertInfo resource);
		public IResourceManager AddResources(IEnumerable<ResourceInsertInfo> resources);
		public IResourceManager UpdateResource(string resourceName, IGPUObject resource);
		public IResourceManager UpdateResource(ulong resourceId, IGPUObject resource);
	}
}
