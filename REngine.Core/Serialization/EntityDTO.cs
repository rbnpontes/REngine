using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Serialization
{
	public struct EntityComponentEntry
	{
		public int Type;
		public int ComponentId;

		public EntityComponentEntry()
		{
			Type = ComponentId = 0;
		}
	}
	public struct ComponentEntry
	{
		public int Id;
		public object Value;
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

	public struct EntitiesDTO
	{
		public int AllocSize;
		public EntityDTO[] Entities;
		public IDictionary<int, ComponentEntry[]> Components;
	}
}
