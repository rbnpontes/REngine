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
		public int Id;
		public ResourceType ResourceType;
		public IGPUObject Resource;
	}
	public interface IResourceManager
	{
		public IResource GetResource(string name);
		public IResource GetResource(int resourceId);
		public IResourceManager AddResource(in ResourceInsertInfo resource);
		public IResourceManager AddResources(IEnumerable<ResourceInsertInfo> resources);
	}
}
