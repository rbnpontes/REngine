using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal static class LookupObjects
	{
		private static ConditionalWeakTable<object, INativeObject> pLookupTable = new ConditionalWeakTable<object, INativeObject>();
		public static void RegisterObject(object key, INativeObject value)
		{
			pLookupTable.AddOrUpdate(key, value);
		}
		public static INativeObject? GetObject(object key)
		{
			INativeObject? value;
			pLookupTable.TryGetValue(key, out value);
			return value;
		}
	}
}
