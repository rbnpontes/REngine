using REngine.Core.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Serialization
{
	public interface IComponentSerializer
	{
		public Component Create();
		public Type GetSerializerType();

		public object OnSerialize(Component component);
		public Component OnDeserialize(Entity entity, object data);
	}

	public sealed class ComponentSerializer
	{
		private readonly Dictionary<int, IComponentSerializer> pSerializers = new Dictionary<int, IComponentSerializer>();

		public static int GetTypeHashCode(Type type)
		{
			return (type.FullName ?? type.Name).GetHashCode();
		}

		public ComponentSerializer AddSerializer<TComponent>(IComponentSerializer serializer) where TComponent : Component
		{
			return AddSerializer(typeof(TComponent), serializer);
		}
		public ComponentSerializer AddSerializer(Type componentType, IComponentSerializer serializer)
		{
			if (!componentType.IsAssignableTo(typeof(Component)))
				throw new ArgumentException("Component Type must be assignable to Component.");

			pSerializers[GetTypeHashCode(componentType)] = serializer;
			return this;
		}

		public Component Create<TComponent>() where TComponent : Component
		{
			return Create(typeof(TComponent));
		}
		public Component Create(Type type)
		{
			if (pSerializers.TryGetValue(GetTypeHashCode(type), out var serializer))
				return serializer.Create();
			throw new ArgumentException("Can´t create this component type. No component serializer was found");
		}

		public object Serialize(Component component)
		{
			if (!pSerializers.TryGetValue(GetTypeHashCode(component.GetType()), out var serializer))
				throw new ArgumentException($"Not found a serializer for type '{component}'.");
			return serializer.OnSerialize(component);
		}
		public Component Deserialize(int componentType, Entity entity, object data)
		{
			if (!pSerializers.TryGetValue(componentType, out var serializer))
				throw new ArgumentException($"Not found a serializer for type code '{componentType}'.");
			return serializer.OnDeserialize(entity, data);
		}
	}
}
