using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.WorldManagement
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ComponentSerializerAttribute : Attribute
	{
		public Type ResolverType { get; private set; }
		public ComponentSerializerAttribute(Type resolver)
		{
			if (!resolver.IsAssignableTo(typeof(IComponentSerializer)))
				throw new Exception($"Resolver type must implement '{nameof(IComponentSerializer)}'");
			ResolverType = resolver;
		}
	}
}
