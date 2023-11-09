using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Serialization
{
	public struct EntityComponentEntry
	{
		public ulong Type;
		public int ComponentId;
		public bool Enabled;

		public EntityComponentEntry()
		{
			Type = 0;
			ComponentId = 0;
			Enabled = false;
		}
	}

	public struct EntityDTO
	{
		public int Id;
		public bool Enabled;
		public string Name;
		public string[] Tags;
		public EntityComponentEntry[] Components;

		public EntityDTO()
		{
			Id = 0;
			Enabled = true;
			Name = string.Empty;
			Tags = Array.Empty<string>();
			Components = Array.Empty<EntityComponentEntry>();
		}
	}

	public struct EntitySerializationData<ComponentType>
	{
		public EntityDTO[] Entities;
		public IDictionary<ulong, List<ComponentType>> Components;
	}
}
