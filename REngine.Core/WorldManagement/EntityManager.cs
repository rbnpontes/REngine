using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
		public bool Enabled;
		public string Name;
		public HashSet<string> Tags;
		public Dictionary<Type, Component> Components;
		public Entity? TargetEntity;

		public EntityData()
		{
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
		private readonly ComponentSerializerFactory pSerializerFactory;
		private readonly ILogger<EntityManager> pLogger;
		private readonly IServiceProvider pServiceProvider;

		public EntityManager(
			EngineSettings engineSettings,
			ILoggerFactory loggerFactory,
			ComponentSerializerFactory serializerFactory,
			IServiceProvider provider
		) : base((int)engineSettings.InitialEntityCount)
		{
			pEntityExpansionLength = (int)Math.Max(Math.Floor(engineSettings.EntityExpansionRate * engineSettings.InitialEntityCount), 1);
			pSerializerFactory = serializerFactory;
			pLogger = loggerFactory.Build<EntityManager>();
			pServiceProvider = provider;
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

		public T CreateComponent<T>(Entity entity) where T : Component
		{
			if (CreateComponent(entity, typeof(T)) is not T component)
				throw new NullReferenceException("Error has occurred at create component.");
			return component;
		}

		public Component CreateComponent(Entity entity, Type type)
		{
			if (!type.IsAssignableTo(typeof(Component)))
				throw new ArgumentException($"Type must inherit {nameof(Component)}", nameof(type));
			var resolver = pSerializerFactory.FindSerializer(ComponentSerializerFactory.GetTypeCode(type));
			if (resolver is null)
				pSerializerFactory.CollectSerializers();
			resolver = pSerializerFactory.GetSerializer(type);
			var component = resolver.Create();
			entity.AddComponent(component);
			return component;
		}

		public bool IsEnabled(Entity entity)
		{
			return pData[entity.Id].Enabled;
		}
		public void SetEnabled(Entity entity, bool enabled)
		{
			pData[entity.Id].Enabled = enabled;
			foreach (var pair in pData[entity.Id].Components)
				pair.Value.OnOwnerChangeVisibility(enabled);
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
			if(File.Exists(filePath))
				File.Delete(filePath);

			using (FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
				return Save(stream);
		}
		public EntityManager Load(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException();
			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return Load(stream);
		}

		public EntityManager Save(Stream stream)
		{
			var jsonSerializer = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				ContractResolver = new EntityContractResolver(pServiceProvider)
			};
			EntitySerializationData<object> serializerData = new ();

			List<EntityDTO> entities = new();
			Dictionary<ulong, List<object>> componentsData = new ();
			HashSet<IComponentSerializer> usedSerializers = new ();

			for(int i =0; i < pData.Length; ++i)
			{
				var entity = pData[i];
				if (entity.TargetEntity is null)
					continue;

				var componentEntries = new List<EntityComponentEntry>();
				foreach(var component in entity.Components)
				{
					var componentCode = ComponentSerializerFactory.GetTypeCode(component.Key);
					if(!componentsData.TryGetValue(componentCode, out var data))
					{
						data = new List<object>();
						componentsData[componentCode] = data;
					}

					var serializer = pSerializerFactory.GetSerializer(component.Key);
					if (!usedSerializers.Contains(serializer))
					{
						serializer.OnBeforeSerialize();
						usedSerializers.Add(serializer);
					}

					data.Add(serializer.OnSerialize(component.Value));

					componentEntries.Add(new EntityComponentEntry
					{
						ComponentId = data.Count - 1,
						Type = componentCode,
						Enabled = component.Value.Enabled
					});
				}

				entities.Add(new EntityDTO { 
					Components = componentEntries.ToArray(),
					Enabled = entity.Enabled,
					Id = i,
					Name = entity.Name,
					Tags = entity.Tags.ToArray()
				});
			}

			serializerData.Entities = entities.ToArray();
			serializerData.Components = componentsData;

			foreach (var serializer in usedSerializers)
				serializer.OnAfterSerialize();

			using (TextWriter writer = new StreamWriter(stream))
			{
				MemoryTraceWriter traceWriter = new();
				jsonSerializer.TraceWriter = traceWriter;
				writer.Write(
					JsonConvert.SerializeObject(serializerData, jsonSerializer)
				);
				pLogger.Debug(traceWriter);
			}

			serializerData.Entities = Array.Empty<EntityDTO>();
			componentsData.Clear();
			entities.Clear();
			usedSerializers.Clear();

			GC.Collect();
			return this;
		}

		public EntityManager Load(Stream stream)
		{
			var jsonSerializer = new JsonSerializer()
			{
				ContractResolver = new EntityContractResolver(pServiceProvider),
				NullValueHandling = NullValueHandling.Ignore,
			};

			string json;
			using (TextReader reader = new StreamReader(stream))
				json = reader.ReadToEnd();

			MemoryTraceWriter traceWriter = new();
			var serializerData = JsonConvert.DeserializeObject<EntitySerializationData<JObject>>(json, new JsonSerializerSettings { 
				TraceWriter = traceWriter
			});
			pLogger.Debug(traceWriter);

			Expand(serializerData.Entities.Length - pAvailableIdx.Count);

			Dictionary<ulong, Component[]> createdComponents = new();
			foreach(var componentPair in serializerData.Components)
			{
				var serializer = (pSerializerFactory.FindSerializer(componentPair.Key) ?? pSerializerFactory.CollectSerializers().FindSerializer(componentPair.Key)) 
				                 ?? throw new EntityException("Not found serializer while is deserializing components");
				serializer.OnBeforeDeserialize();

				var components = new Component[componentPair.Value.Count];
				for(var i =0; i < componentPair.Value.Count; i++)
				{
					var data = componentPair.Value[i].ToObject(serializer.GetSerializeType(), jsonSerializer) 
					           ?? throw new NullReferenceException($"Can´t deserialize component data type '{serializer.GetSerializeType().Name}'");
					components[i] = serializer.OnDeserialize(data);
				}

				serializer.OnAfterDeserialize();

				createdComponents[componentPair.Key] = components;
			}

			foreach(var entityItem in serializerData.Entities)
			{
				var entity = CreateEntity(entityItem.Name);
				foreach (var tag in entityItem.Tags)
					entity.AddTag(tag);

				entity.Enabled = entityItem.Enabled;

				var entityData = pData[entity.Id];
				foreach(var componentEntry in entityItem.Components)
				{
					var component = createdComponents[componentEntry.Type][componentEntry.ComponentId];
					component.Enabled = componentEntry.Enabled;

					entityData.Components[component.GetType()] = component;
					
					component.Owner = entity;
				}
				pData[entity.Id] = entityData;

				// Execute Setup
				foreach (var pair in entityData.Components)
					pair.Value.OnSetup();
			}

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
