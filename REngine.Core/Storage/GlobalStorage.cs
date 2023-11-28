using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Storage
{
	public static class GlobalStorage
	{
		private static readonly Dictionary<string, object?> sItems = [];

		public static string[] Keys => [.. sItems.Keys];
		public static object?[] Values => [.. sItems.Values];
		public static void SetItem<T>(string key, T? value)
		{
			sItems[key] = value;
		}

		public static T? GetItem<T>(string key)
		{
			sItems.TryGetValue(key, out var value);
			if (value is null)
				return default(T?);
			return (T?)value;
		}

		public static void RemoveItem(string key)
		{
			sItems.Remove(key);
		}
	}
}
