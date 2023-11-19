using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Serialization
{
	internal class CoreContractResolver : DefaultContractResolver
	{
		public static CoreContractResolver Instance { get; } = new();

		public IServiceProvider? ServiceProvider { get; set; }

		private CoreContractResolver() { }

		private CoreContractResolver(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		protected override JsonObjectContract CreateObjectContract(Type objectType)
		{
			if(ServiceProvider is null)
				return base.CreateObjectContract(objectType);

			var target = ServiceProvider.GetService(objectType);
			if(target is null)
				return base.CreateObjectContract(objectType);

			var contract = base.CreateObjectContract(objectType);
			contract.DefaultCreator = () => target;
			return contract;
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);
			if (member.GetCustomAttribute<SerializationIgnoreAttribute>() != null)
				property.ShouldSerialize = property.ShouldDeserialize = (instance) => false;
			return property;
		}

		public static CoreContractResolver Build(IServiceProvider provider)
		{
			return new CoreContractResolver(provider);
		}
	}
}
