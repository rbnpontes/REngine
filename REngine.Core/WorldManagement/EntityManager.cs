using REngine.Core.IO;
using REngine.Core.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	public struct EntityData
	{
		public int Id;
		public bool Enabled;
		public string Name;
		public HashSet<string> Tags;
		public Dictionary<Type, Component> Components;
		public Entity? TargetEntity;

		public EntityData()
		{
			Id = 0;
			Enabled = true;
			Name = string.Empty;
			Tags = new HashSet<string>();
			Components = new Dictionary<Type, Component>();
			TargetEntity = null;
		}
	}

	public sealed class EntityManager : BaseSystem<EntityData>, IEnumerable<Entity>
	{
		private readonly int pEntityExpansionLength;
		private readonly ComponentSerializer pComponentSerializer;
		private readonly ILogger<EntityManager> pLogger;

		public EntityManager(
			EngineSettings engineSettings,
			ComponentSerializer componentSerializer,
			ILoggerFactory loggerFactory
		) : base((int)engineSettings.InitialEntityCount)
		{
			pEntityExpansionLength = (int)Math.Max(Math.Floor(engineSettings.EntityExpansionRate * engineSettings.InitialEntityCount), 1);
			pComponentSerializer = componentSerializer;
			pLogger = loggerFactory.Build<EntityManager>();
		}

		public EntityManager Destroy(Entity entity)
		{
			if (entity.IsDisposed)
				return this;

			int id = entity.Id;
			if (id >= pData.Length)
				throw new InvalidEntityIdException("Entity Id is greater than Entity Pool");
			if (id < 0)
				throw new InvalidEntityIdException("Entity Id cannot be negative value");

			var data = pData[id];
			foreach (var pair in data.Components)
				pair.Value.Dispose();

			pData[id] = new EntityData();

			pAvailableIdx.Enqueue(id);

			return this;
		}

		public EntityManager DestroyAll()
		{
			for(int i =0; i < pData.Length; ++i)
			{
				var entity = pData[i];
				if (entity.TargetEntity is null)
					continue;
				Destroy(entity.TargetEntity);
			}
			return this;
		}

		public Entity CreateEntity(string? name = null)
		{
			int id = Acquire();
			Entity entity = new Entity(this, id);
			EntityData data = new EntityData();
			data.Name = name ?? string.Empty;
			data.TargetEntity = entity;
			pData[id] = data;
			return entity;
		}

		public Entity[] GetEntities()
		{
			Entity[] entities = new Entity[pData.Length - pAvailableIdx.Count];
			int nextId = 0;
			for(int i =0; i < pData.Length; ++i)
			{
				var entityData = pData[i];
				if (entityData.TargetEntity is null)
					continue;
				entities[nextId] = entityData.TargetEntity;
				++nextId;
			}

			return entities;
		}
		
		public Entity GetEntity(int id)
		{
			if (id >= pData.Length)
				throw new InvalidEntityIdException("Id is greater than Entity Pool");
			if (id < 0)
				throw new InvalidEntityIdException("Id cannot be negative");

			var entity = pData[id].TargetEntity;
			if (entity is null)
				throw new EntityException("Invalid Entity Id. It seems this entity has been previous destroyed or not be allocated yet.");
			return entity;
		}

		/// <summary>
		/// Optimize will rearrange the whole poll
		/// to fit only at created entities
		/// Call this only if you don't have plans to create
		/// new entities along games, otherwise you will pay
		/// for poll expansion
		/// </summary>
		/// <returns></returns>
		public EntityManager Optimize()
		{
			// If available Ids is empty, then pool has already optimized
			if (pData.Length == 0)
				return this;

			int newLength = pData.Length - pAvailableIdx.Count;
			pAvailableIdx.Clear();

			EntityData[] newEntities = new EntityData[newLength];
			int nextId = 0;
			for (int i = 0; i < pData.Length; ++i)
			{
				var data = pData[i];
				if (data.TargetEntity is null)
					continue;
				newEntities[nextId] = data;
				++nextId;
			}
			pData = newEntities;

			GC.Collect();
			return this;
		}

		/// <summary>
		/// Reserve some new slots on Entity Poll
		/// if you want to create a lot of entities
		/// Then you need to call this method first to
		/// reduce overhead
		/// </summary>
		/// <param name="newSlots"></param>
		/// <returns></returns>
		public EntityManager Reserve(int newSlots)
		{
			if (newSlots < 1)
				throw new EntityException($"New slots value cannot be less than 1");
			Expand(newSlots);
			return this;
		}

		/// <summary>
		/// Iterates over all available entities
		/// </summary>
		/// <param name="action"></param>
		public void ForEach(Action<Entity> action)
		{
			foreach(var entity in this)
				action(entity);
		}

		/// <summary>
		/// Save All Entities and Components to a File
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public EntityManager Save(string filePath)
		{
			using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
				return Save(stream);
		}
		public EntityManager Save(Stream stream)
		{
			EntitiesDTO entitiesDTO = new EntitiesDTO();
			entitiesDTO.AllocSize = pData.Length;
			
			List<EntityDTO> entities = new List<EntityDTO>();
			Dictionary<int, List<Component>> components2Insert = new Dictionary<int, List<Component>>();
			Dictionary<int, ComponentEntry[]> componentEntries = new Dictionary<int, ComponentEntry[]>();

			// Process Entities
			foreach(var entity in pData)
			{
				if (entity.TargetEntity is null)
					continue;
				entities.Add(new EntityDTO
				{
					Id = entity.Id,
					Enabled = entity.Enabled,
					Name = entity.Name,
					Tags = entity.Tags.ToArray(),
					Components = entity.Components.Select(x =>
					{
						return new EntityComponentEntry
						{
							Type = ComponentSerializer.GetTypeHashCode(x.Key),
							ComponentId = x.Value.GetHashCode()
						};
					}).ToArray()
				});

				foreach(var component in entity.Components)
				{
					int componentKey = ComponentSerializer.GetTypeHashCode(component.Key);
					if (!components2Insert.TryGetValue(componentKey, out var componentList))
						components2Insert[componentKey] = componentList = new List<Component>();

					componentList.Add(component.Value);
				}
			}
			// Process Components
			foreach(var componentPair in components2Insert)
			{
				var data = pComponentSerializer.Serialize(componentPair.Key, componentPair.Value);
				if (data.Count() != componentPair.Value.Count())
					throw new EntityException($"Invalid Serialization of Component. Serialized Component Data count must be equal to components count.");

				ComponentEntry[] entries = new ComponentEntry[data.Count()];
				for(int i =0; i< data.Count(); i++)
				{
					entries[i] = new ComponentEntry
					{
						Id = componentPair.Value[i].GetHashCode(),
						Value = data.ElementAt(i)
					};
				}

				componentEntries[componentPair.Key] = entries;
			}

			entitiesDTO.Entities = entities.ToArray();
			entitiesDTO.Components = componentEntries;

			using (TextWriter writer = new StreamWriter(stream))
				writer.Write(entitiesDTO.ToJson());
			return this;
		}

		public EntityManager Dump()
		{
			StringBuilder output = new();
			output.AppendLine($"Allocated Entities: {pData.Length}");
			output.AppendLine($"Used Entities: {pData.Length - pAvailableIdx.Count}");
			output.AppendLine($"Available Space: {pAvailableIdx.Count}");
			output.AppendLine("Entities:");
			for(int i =0; i < pData.Length; ++i)
			{
				if (pData[i].TargetEntity is null)
					continue;
				var data = pData[i];
				output.AppendLine($"- [{i}]:");
				output.AppendLine($"\tEnabled: {data.Enabled}");
				output.AppendLine($"\tName: {data.Name}");
				output.AppendLine($"\tTags: {string.Join(",", data.Tags)}");
				output.AppendLine($"\tComponents:");
				foreach(var component in data.Components)
				{
					Type componentType = component.Value.GetType();
					output.AppendLine($"\t\t[{component.Value.GetHashCode()}]: {(componentType.FullName ?? componentType.Name)}");
				}
			}

			pLogger.Info(output);
			return this;
		}

		internal void GetEntityData(int index, out EntityData output)
		{
			output = pData[index];
		}
		internal void SetEntityData(int index, in EntityData input)
		{
			pData[index] = input;
		}

		protected override void OnAllocate(int requestSize)
		{
			base.OnAllocate(requestSize);
			GC.Collect();
		}
		protected override int GetExpansionSize()
		{
			return pEntityExpansionLength;
		}

		public IEnumerator<Entity> GetEnumerator()
		{
			// If there's no entity allocated, there's no reason
			// to loop over there.
			if (pData.Length == pAvailableIdx.Count)
				yield break;

			foreach (var entity in pData)
			{
				if (entity.TargetEntity is null)
					continue;
				yield return entity.TargetEntity;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
