using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.WorldManagement;

namespace REngine.Core.Serialization
{
	internal class EntityContractResolver : DefaultContractResolver
	{
		private readonly IServiceProvider pServiceProvider;
		private readonly ComponentSerializerFactory pComponentSerializerFactory;

		public EntityContractResolver(IServiceProvider serviceProvider)
		{
			pServiceProvider = serviceProvider;
			pComponentSerializerFactory = serviceProvider.Get<ComponentSerializerFactory>();
		}

		protected override JsonObjectContract CreateObjectContract(Type objectType)
		{
			JsonObjectContract contract = base.CreateObjectContract(objectType);
			var target = pServiceProvider.GetService(objectType);
			if (target is not null)
			{
				contract.DefaultCreator = () => target;
				return contract;
			}

			if(!objectType.IsAssignableTo(typeof(Component)))
				return contract;

			var ctor = objectType.GetConstructors().FirstOrDefault();
			if (ctor is null)
				return contract;

			var parameters = ctor.GetParameters();
			if (parameters.Length == 0)
				return contract;

			// resolve parameters
			var resolvedParams = parameters.Select(x => pServiceProvider.GetService(x.ParameterType));
			contract.DefaultCreator = () => Activator.CreateInstance(objectType, resolvedParams.ToArray()) 
			                                ?? throw new NullReferenceException($"Could not possible to create type '{objectType.FullName ?? objectType.Name}'");
			return contract;
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			if (member.GetCustomAttribute<SerializationIgnoreAttribute>() != null)
				property.ShouldSerialize = property.ShouldDeserialize = (instance) => false;
			return property;
		}
	}
}
