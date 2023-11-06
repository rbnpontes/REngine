using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ComponentAttribute : Attribute
	{
		public Type? Resolver { get; private set; }

		public ComponentAttribute()
		{
		}
		public ComponentAttribute(Type resolver)
		{
			if (resolver.IsAssignableTo(typeof(IComponentResolver)))
				throw new Exception($"Resolver type must implement '{nameof(IComponentResolver)}'");
			Resolver = resolver;
		}
	}
}
