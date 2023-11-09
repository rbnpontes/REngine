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
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}
		public static T? FromJson<T>(this string data)
		{
			return JsonConvert.DeserializeObject<T>(data);
		}
	}
}
