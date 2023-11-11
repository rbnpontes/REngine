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
		private static readonly CoreContractResolver sInstance = new CoreContractResolver();
		public static CoreContractResolver Instance { get => sInstance; }
		private CoreContractResolver() { }

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);
			if (member.GetCustomAttribute<SerializationIgnoreAttribute>() != null)
				property.ShouldSerialize = property.ShouldDeserialize = (instance) => false;
			return property;
		}
	}
}
