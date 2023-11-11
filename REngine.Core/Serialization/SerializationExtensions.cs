using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Serialization
{
	public static class SerializationExtensions
	{
		public static string ToJson(this object obj)
		{
			return JsonConvert.SerializeObject(obj,new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				ContractResolver = CoreContractResolver.Instance
			});
		}
		public static T? FromJson<T>(this string data)
		{
			return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings { 
				ContractResolver = CoreContractResolver.Instance
			});
		}
	}
}
